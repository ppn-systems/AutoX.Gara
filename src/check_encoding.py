
import os

root_dir = r'e:\Cs\AutoX.Gara\src'
missing_bom = []
invalid_utf8 = []

for root, dirs, files in os.walk(root_dir):
    for d in ['bin', 'obj', '.vs', '.git']:
        if d in dirs:
            dirs.remove(d)
            
    for file in files:
        if file.endswith('.cs'):
            path = os.path.join(root, file)
            try:
                with open(path, 'rb') as f:
                    header = f.read(3)
                    f.seek(0)
                    content_raw = f.read()
                
                # Check for BOM
                has_bom = header == b'\xef\xbb\xbf'
                
                # Check if it's valid UTF-8
                try:
                    content_raw.decode('utf-8')
                    is_utf8 = True
                except:
                    is_utf8 = False
                
                # If it has non-ASCII characters but no BOM, it's a candidate for issues
                has_non_ascii = any(b > 127 for b in content_raw)
                
                if has_non_ascii and not has_bom:
                    missing_bom.append(path)
                
                if not is_utf8 and not has_bom:
                    invalid_utf8.append(path)
                    
            except Exception as e:
                print(f"Error reading {path}: {e}")

print("--- Files with non-ASCII but NO BOM ---")
for p in missing_bom:
    print(p)

print("\n--- Files that are NOT valid UTF-8 (and NO BOM) ---")
for p in invalid_utf8:
    print(p)
