import json
import urllib.parse
import re

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
            # Normalize URL to /api/v1/... format
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

def generate_postman_collection(swagger_data):
    collection = {
        "info": {
            "name": "Resala BackEnd Collection v4 (Auto-Generated)",
            "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
            "description": "Full Postman collection based on the latest swagger definition."
        },
        "item": [],
        "variable": []
    }
    
    tags = {}
    paths = swagger_data.get('paths', {})
    
    for path, methods in paths.items():
        for method, operation in methods.items():
            if method.lower() not in ['get', 'post', 'put', 'delete', 'patch']:
                continue
            
            op_tags = operation.get('tags', ['Default'])
            tag = op_tags[0] if op_tags else 'Default'
            
            if tag not in tags:
                tags[tag] = []
                
            req = {
                "name": operation.get('summary', operation.get('operationId', f'{method.upper()} {path}')),
                "request": {
                    "method": method.upper(),
                    "header": [{"key": "Accept", "value": "application/json"}],
                    "url": {
                        "raw": f"{{{{baseUrl}}}}{path}",
                        "host": ["{{baseUrl}}"],
                        "path": [p for p in path.split('/') if p]
                    }
                },
                "response": []
            }
            
            # Add security
            if 'security' in operation or 'security' in swagger_data:
                req["request"]["auth"] = {
                    "type": "bearer",
                    "bearer": [
                        {
                            "key": "token",
                            "value": "{{token}}",
                            "type": "string"
                        }
                    ]
                }
                
            # Parameters (Path, Query)
            query_params = []
            for param in operation.get('parameters', []):
                if param.get('in') == 'query':
                    query_params.append({
                        "key": param.get('name'),
                        "value": f"{{{{{param.get('name')}}}}}",
                        "description": param.get('description', '')
                    })
                elif param.get('in') == 'path':
                    # Modify url paths to use variables
                    for i, p in enumerate(req['request']['url']['path']):
                        if p == f"{{{param.get('name')}}}":
                            req['request']['url']['path'][i] = f":{param.get('name')}"
            
            if query_params:
                req['request']['url']['query'] = query_params
                req['request']['url']['raw'] += "?" + "&".join([f"{qp['key']}={qp['value']}" for qp in query_params])
            
            # Request Body
            if 'requestBody' in operation:
                content = operation['requestBody'].get('content', {})
                if 'application/json' in content:
                    schema_ref = content['application/json'].get('schema', {}).get('$ref')
                    body_schema = get_schema_from_ref(schema_ref, swagger_data)
                    example_body = generate_example_from_schema(body_schema, swagger_data)
                    req["request"]["body"] = {
                        "mode": "raw",
                        "raw": json.dumps(example_body, indent=2),
                        "options": {
                            "raw": {
                                "language": "json"
                            }
                        }
                    }
                elif 'multipart/form-data' in content:
                    req["request"]["body"] = {
                        "mode": "formdata",
                        "formdata": [
                            {"key": "file", "type": "file", "src": []}
                        ]
                    }
            
            tags[tag].append(req)

    for tag, items in tags.items():
        collection['item'].append({
            "name": tag,
            "item": items
        })
        
    return collection

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
    
    swagger_data = parse_swagger(swagger_path)
    postman_data = parse_postman(postman_path)
    
    old_endpoints = extract_endpoints_from_postman(postman_data.get('item', []))
    old_map = {f"{ep['method']}:{ep['path']}": ep for ep in old_endpoints}
    
    new_endpoints = []
    swagger_paths = swagger_data.get('paths', {})
    for path, methods in swagger_paths.items():
        for method in methods.keys():
            if method.lower() not in ['get', 'post', 'put', 'delete', 'patch']:
                continue
            new_endpoints.append({
                'method': method.upper(),
                'path': path
            })
    new_map = {f"{ep['method']}:{ep['path']}": ep for ep in new_endpoints}
    
    documented = []
    missing_from_postman = []
    deprecated = []
    
    for key, ep in new_map.items():
        if key in old_map:
            documented.append(ep)
        else:
            missing_from_postman.append(ep)
            
    for key, ep in old_map.items():
        if key not in new_map:
            deprecated.append(ep)
            
    # Audit Report
    with open('Audit_Report.md', 'w', encoding='utf-8') as f:
        f.write("# API Audit Report\n\n")
        f.write(f"## ✅ Documented Endpoints ({len(documented)})\n")
        for ep in documented:
            f.write(f"- `{ep['method']}` {ep['path']}\n")
            
        f.write(f"\n## ❌ Missing from Old Postman (New/Undocumented) ({len(missing_from_postman)})\n")
        for ep in missing_from_postman:
            f.write(f"- `{ep['method']}` {ep['path']}\n")
            
        f.write(f"\n## 🗑 Deprecated APIs (In Old Postman, Not in Swagger) ({len(deprecated)})\n")
        for ep in deprecated:
            f.write(f"- `{ep['method']}` {ep['path']}\n")

    # Generate New Collection
    new_collection = generate_postman_collection(swagger_data)
    with open('Resala_Complete_v4.postman_collection.json', 'w', encoding='utf-8') as f:
        json.dump(new_collection, f, indent=2, ensure_ascii=False)

    # Generate Environment
    env = {
        "name": "Resala Environment",
        "values": [
            {"key": "baseUrl", "value": "https://localhost:7154", "type": "default", "enabled": True},
            {"key": "token", "value": "", "type": "secret", "enabled": True},
            {"key": "adminToken", "value": "", "type": "secret", "enabled": True},
            {"key": "donorToken", "value": "", "type": "secret", "enabled": True}
        ]
    }
    with open('Resala_Environment.postman_environment.json', 'w', encoding='utf-8') as f:
        json.dump(env, f, indent=2, ensure_ascii=False)
        
    print(f"Generated Audit_Report.md")
    print(f"Generated Resala_Complete_v4.postman_collection.json")
    print(f"Generated Resala_Environment.postman_environment.json")
    print(f"Stats: {len(documented)} documented, {len(missing_from_postman)} missing, {len(deprecated)} deprecated.")

if __name__ == '__main__':
    main()
