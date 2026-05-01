import json
import urllib.parse
import re
import os

def parse_swagger(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        return json.load(f)

def parse_postman(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        return json.load(f)

def extract_endpoints_from_postman(items, base_url_var='{{baseUrl}}'):
    endpoints = []
    for item in items:
        if 'item' in item:
            endpoints.extend(extract_endpoints_from_postman(item['item'], base_url_var))
        elif 'request' in item:
            req = item['request']
            method = req.get('method', '').upper()
            url = req.get('url', {})
            if isinstance(url, dict):
                raw = url.get('raw', '')
            else:
                raw = url
            path = raw.replace(base_url_var, '').split('?')[0]
            if not path.startswith('/'):
                path = '/' + path
            
            # Postman variables might be {{id}} or :id, convert to swagger {id}
            path = re.sub(r':([a-zA-Z0-9_]+)', r'{\1}', path)
            path = re.sub(r'{{([a-zA-Z0-9_]+)}}', r'{\1}', path)
            
            endpoints.append({
                'method': method,
                'path': path,
                'name': item.get('name', 'Unnamed Request'),
                'raw': raw
            })
    return endpoints

def scan_cs_controllers(controllers_dir):
    cs_endpoints = []
    # e.g., [HttpPost("{id:int}/pay/electronic")]
    pattern = re.compile(r'\[Http(Get|Post|Put|Delete|Patch)(?:\(\s*"(.*?)"\s*\))?\]', re.IGNORECASE)
    # e.g., [Route("api/v1/[controller]")]
    base_pattern = re.compile(r'\[Route\(\s*"(.*?)"\s*\)\]')
    
    for root, dirs, files in os.walk(controllers_dir):
        for file in files:
            if file.endswith('.cs'):
                content = open(os.path.join(root, file), 'r', encoding='utf-8').read()
                base_routes = base_pattern.findall(content)
                base_route = base_routes[0] if base_routes else ''
                
                # resolve [controller]
                controller_name = file.replace('Controller.cs', '')
                base_route = base_route.replace('[controller]', controller_name.lower())
                
                # Check for base crud endpoints from BaseCrudController
                # Not doing AST, just basic regex. We can at least catch explicit [Http*] 
                
                matches = pattern.findall(content)
                for m in matches:
                    method = m[0].upper()
                    route = m[1] if len(m)>1 else ''
                    
                    # Normalize route parameters like {id:int} to {id}
                    route = re.sub(r'{([a-zA-Z0-9_]+):[^}]+}', r'{\1}', route)
                    
                    full_route = '/' + base_route
                    if route and route != '':
                        if route.startswith('/'):
                            full_route += route
                        else:
                            full_route += '/' + route
                    
                    full_route = full_route.replace('//', '/')
                    
                    cs_endpoints.append({
                        'method': method,
                        'path': full_route,
                        'controller': controller_name
                    })
    return cs_endpoints

def get_schema_from_ref(ref, swagger_data):
    if not ref:
        return {}
    parts = ref.split('/')
    if len(parts) == 4 and parts[0] == '#' and parts[1] == 'components' and parts[2] == 'schemas':
        return swagger_data.get('components', {}).get('schemas', {}).get(parts[3], {})
    return {}

def generate_example_from_schema(schema, swagger_data):
    if not schema:
        return {}
    if 'type' in schema:
        if schema['type'] == 'object':
            props = schema.get('properties', {})
            return {k: generate_example_from_schema(v, swagger_data) for k, v in props.items()}
        elif schema['type'] == 'array':
            return [generate_example_from_schema(schema.get('items', {}), swagger_data)]
        elif schema['type'] == 'string':
            if schema.get('format') == 'date-time':
                return "2026-04-28T12:00:00Z"
            return "string"
        elif schema['type'] == 'integer':
            return 0
        elif schema['type'] == 'boolean':
            return True
        elif schema['type'] == 'number':
            return 0.0
    
    if 'allOf' in schema:
        res = {}
        for s in schema['allOf']:
            if '$ref' in s:
                res.update(generate_example_from_schema(get_schema_from_ref(s['$ref'], swagger_data), swagger_data))
            else:
                res.update(generate_example_from_schema(s, swagger_data))
        return res

    if '$ref' in schema:
        return generate_example_from_schema(get_schema_from_ref(schema['$ref'], swagger_data), swagger_data)
    
    return "any"

def main():
    swagger_path = 'D:/Resala-BackEnd/swagger.json'
    postman_path = 'D:/Resala-BackEnd/Resala_Complete_v3.postman_collection.json'
    controllers_dir = 'D:/Resala-BackEnd/BackEndApi/Controllers'
    
    swagger_data = parse_swagger(swagger_path)
    postman_data = parse_postman(postman_path)
    cs_endpoints = scan_cs_controllers(controllers_dir)
    
    old_endpoints = extract_endpoints_from_postman(postman_data.get('item', []))
    old_map = {f"{ep['method']}:{ep['path']}": ep for ep in old_endpoints}
    
    # Base from swagger
    swagger_paths = swagger_data.get('paths', {})
    combined_endpoints = {}
    
    # 1. Add swagger endpoints
    for path, methods in swagger_paths.items():
        for method, operation in methods.items():
            if method.lower() not in ['get', 'post', 'put', 'delete', 'patch']:
                continue
            key = f"{method.upper()}:{path}"
            combined_endpoints[key] = {
                'method': method.upper(),
                'path': path,
                'source': 'swagger',
                'operation': operation,
                'tag': operation.get('tags', ['Default'])[0] if operation.get('tags') else 'Default',
                'summary': operation.get('summary', f"{method.upper()} {path}")
            }
            
    # 2. Add C# endpoints (if missing in swagger)
    new_from_cs = []
    for ep in cs_endpoints:
        key = f"{ep['method']}:{ep['path']}"
        if key not in combined_endpoints:
            # Also check base paths without trailing slashes
            alt_key = key.rstrip('/')
            if alt_key in combined_endpoints:
                continue
                
            combined_endpoints[key] = {
                'method': ep['method'],
                'path': ep['path'],
                'source': 'code',
                'operation': {},
                'tag': ep['controller'],
                'summary': f"{ep['method']} {ep['path']} (New)"
            }
            new_from_cs.append(combined_endpoints[key])
            
    # Determine documented, missing, deprecated
    documented = []
    missing_from_postman = []
    deprecated = []
    
    for key, ep in combined_endpoints.items():
        if key in old_map:
            documented.append(ep)
        else:
            missing_from_postman.append(ep)
            
    for key, ep in old_map.items():
        # strict compare might miss {id} vs 1, but we list them
        if key not in combined_endpoints:
            deprecated.append(ep)
            
    # Build Postman Collection
    collection = {
        "info": {
            "name": "Resala BackEnd Collection v4 (Complete)",
            "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
            "description": "Full Postman collection based on swagger + actual C# controllers."
        },
        "item": [],
        "variable": []
    }
    
    tags_map = {}
    for key, ep in combined_endpoints.items():
        tag = ep['tag']
        if tag not in tags_map:
            tags_map[tag] = []
            
        req = {
            "name": ep['summary'],
            "request": {
                "method": ep['method'],
                "header": [{"key": "Accept", "value": "application/json"}],
                "url": {
                    "raw": f"{{{{baseUrl}}}}{ep['path']}",
                    "host": ["{{baseUrl}}"],
                    "path": [p for p in ep['path'].split('/') if p]
                }
            },
            "response": []
        }
        
        # Security
        req["request"]["auth"] = {
            "type": "bearer",
            "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]
        }
        
        # Path variables formatting
        for i, p in enumerate(req['request']['url']['path']):
            if p.startswith('{') and p.endswith('}'):
                var_name = p[1:-1]
                req['request']['url']['path'][i] = f":{var_name}"
                
        # Query params from swagger if available
        op = ep.get('operation', {})
        query_params = []
        for param in op.get('parameters', []):
            if param.get('in') == 'query':
                query_params.append({
                    "key": param.get('name'),
                    "value": f"{{{{{param.get('name')}}}}}",
                    "description": param.get('description', '')
                })
                
        if query_params:
            req['request']['url']['query'] = query_params
            req['request']['url']['raw'] += "?" + "&".join([f"{qp['key']}={qp['value']}" for qp in query_params])
            
        # Body from swagger if available
        if 'requestBody' in op:
            content = op['requestBody'].get('content', {})
            if 'application/json' in content:
                schema_ref = content['application/json'].get('schema', {}).get('$ref')
                body_schema = get_schema_from_ref(schema_ref, swagger_data)
                example_body = generate_example_from_schema(body_schema, swagger_data)
                req["request"]["body"] = {
                    "mode": "raw",
                    "raw": json.dumps(example_body, indent=2),
                    "options": {"raw": {"language": "json"}}
                }
            elif 'multipart/form-data' in content:
                req["request"]["body"] = {
                    "mode": "formdata",
                    "formdata": [{"key": "file", "type": "file", "src": []}]
                }
        else:
            # If no swagger doc, let's at least add an empty body template for POST/PUT
            if ep['method'] in ['POST', 'PUT']:
                req["request"]["body"] = {
                    "mode": "raw",
                    "raw": "{}",
                    "options": {"raw": {"language": "json"}}
                }
                
        tags_map[tag].append(req)

    for tag, items in tags_map.items():
        collection['item'].append({
            "name": tag,
            "item": items
        })

    with open('D:/Resala-BackEnd/Resala_Complete_v4.postman_collection.json', 'w', encoding='utf-8') as f:
        json.dump(collection, f, indent=2, ensure_ascii=False)
        
    env = {
        "name": "Resala Environment",
        "values": [
            {"key": "baseUrl", "value": "https://localhost:7154", "type": "default", "enabled": True},
            {"key": "token", "value": "", "type": "secret", "enabled": True},
            {"key": "adminToken", "value": "", "type": "secret", "enabled": True},
            {"key": "donorToken", "value": "", "type": "secret", "enabled": True}
        ]
    }
    with open('D:/Resala-BackEnd/Resala_Environment.postman_environment.json', 'w', encoding='utf-8') as f:
        json.dump(env, f, indent=2, ensure_ascii=False)

    with open('D:/Resala-BackEnd/Audit_Report.md', 'w', encoding='utf-8') as f:
        f.write("# API Audit Report (Codebase + Swagger Sync)\n\n")
        
        f.write(f"## 🆕 Newly Found Endpoints in Code (Not in Swagger) ({len(new_from_cs)})\n")
        f.write("These endpoints exist in C# Controllers but are missing from `swagger.json`.\n")
        for ep in new_from_cs:
            f.write(f"- `{ep['method']}` {ep['path']} (Controller: {ep['tag']})\n")
            
        f.write(f"\n## ❌ Missing from Old Postman ({len(missing_from_postman)})\n")
        f.write("Endpoints that are present in the backend (Swagger or Code) but missing from your OLD Postman collection.\n")
        for ep in missing_from_postman:
            f.write(f"- `{ep['method']}` {ep['path']}\n")
            
        f.write(f"\n## 🗑 Deprecated APIs (In Old Postman, Not Found in Code/Swagger) ({len(deprecated)})\n")
        f.write("Endpoints that were in the old Postman collection but not found in the current C# code or Swagger. *(Note: Many of these are just using hardcoded IDs instead of `{id}` parameters)*.\n")
        for ep in deprecated:
            f.write(f"- `{ep['method']}` {ep['path']}\n")

    print(f"Stats: {len(new_from_cs)} purely new from C#, {len(missing_from_postman)} total missing from postman, {len(deprecated)} deprecated/hardcoded old.")

if __name__ == '__main__':
    main()
