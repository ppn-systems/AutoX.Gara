
import os
import re

# Comprehensive mapping for all remaining Mojibake and '?' corruption
# Based on exact grep output
fix_map = {
    "truyá» n": "truyền",
    "Gá» i": "Gọi",
    "vá» ": "về",
    "HÃ£ng": "Hãng",
    "Ä iá» u khoản": "Điều khoản",
    "Chá» n": "Chọn",
    "Ä ang giữ": "Đang giữ",
    "Lá» c": "Lọc",
    "Số tiá» n": "Số tiền",
    "Ä ổi trạng thái": "Đổi trạng thái",
    "Ä ơn giá": "Đơn giá",
    "Ä Ã£ thanh toán": "Đã thanh toán",
    "Theo giá» ": "Theo giờ",
    "thá» i gian chá» ": "thời gian chờ",
    "vu\?t": "vượt",
    "du\?c": "được",
    "kÃ½ t\?": "ký tự",
    "kÃ½ t?": "ký tự",
    "Ä \u1ed5i tr\u1ea1ng th\u00e1i": "Đổi trạng thái",
}

root_dir = r'e:\Cs\AutoX.Gara\src\AutoX.Gara.Frontend'
for root, dirs, files in os.walk(root_dir):
    if 'bin' in dirs: dirs.remove('bin')
    if 'obj' in dirs: dirs.remove('obj')
    for file in files:
        if file.endswith(('.cs', '.xaml')):
            path = os.path.join(root, file)
            try:
                with open(path, 'rb') as f:
                    content_bytes = f.read()
                
                try:
                    content = content_bytes.decode('utf-8')
                except UnicodeDecodeError:
                    content = content_bytes.decode('latin-1')
                
                original = content
                
                for old, new in fix_map.items():
                    content = content.replace(old, new)
                
                if content != original:
                    is_cs = file.endswith('.cs')
                    with open(path, 'w', encoding='utf-8-sig' if is_cs else 'utf-8') as f:
                        f.write(content)
                    print(f'Polished: {file}')
            except Exception as e:
                print(f'Error in {file}: {e}')
