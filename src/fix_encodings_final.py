
import os

fixes = {
    'dnh cho': 'dành cho',
    'dang nh?p': 'đăng nhập',
    'ngu?i dng': 'người dùng',
    'h? th?ng': 'hệ thống',
    'Ch? ch?a': 'Chỉ chứa',
    'thng tin': 'thông tin',
    't?i thi?u': 'tối thiểu',
    'm client': 'mà client',
    'g?i ln': 'gửi lên',
    'Khng luu': 'Không lưu',
    'b?t k?': 'bất kỳ',
    'b?o m?t': 'bảo mật',
    'nh?y c?m': 'nhạy cảm',
    'ti kho?n': 'tài khoản',
    'm?t kh?u': 'mật khẩu',
    'd?ng clear text': 'dạng clear text',
    'ch? d?': 'chỉ để',
    'xc th?c': 'xác thực',
    'm?t l?n': 'một lần',
    's? d?ng': 'sử dụng',
    't? ngu?i dng': 'từ người dùng',
    'luu tr?': 'lưu trữ',
    'Tn': 'Tên',
    'M?t kh?u': 'Mật khẩu',
    'D? li?u': 'Dữ liệu',
    'd? li?u': 'dữ liệu',
    'nghi?p v?': 'nghiệp vụ',
    'k?t ná»‘i': 'kết nối',
    'ph?n há»“i': 'phản hồi',
    'lá»—i': 'lỗi',
    'hợp lá»‡': 'hợp lệ',
    'tAi khon' : 'tài khoản',
    'xAc thc' : 'xác thực',
    'dAnh cho' : 'dành cho',
    'ng?i dA1ng' : 'người dùng',
    'h th`ng' : 'hệ thống',
    't`i thiu' : 'tối thiểu',
    '`ng nh-p' : 'đăng nhập',
    'g-i lAn' : 'gửi lên',
    'bo m-t' : 'bảo mật',
    'm-t khcu' : 'mật khẩu'
}

root = r'e:\Cs\AutoX.Gara\src'
count = 0
for r, d, files in os.walk(root):
    if any(x in r for x in ['bin', 'obj', '.vs']): continue
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(r, f)
            try:
                with open(p, 'rb') as file:
                    raw = file.read()
                
                # Try to decode
                content = None
                for enc in ['utf-8-sig', 'utf-8', 'cp1258', 'latin-1']:
                    try:
                        content = raw.decode(enc)
                        break
                    except:
                        continue
                
                if content is None: continue

                original = content
                for old, new in fixes.items():
                    content = content.replace(old, new)
                
                # Check if change is needed or if BOM is missing
                needs_bom = not raw.startswith(b'\xef\xbb\xbf')
                
                if content != original or needs_bom:
                    with open(p, 'w', encoding='utf-8-sig') as file:
                        file.write(content)
                    count += 1
                    print(f'Fixed: {p}')
            except Exception as e:
                print(f"Error processing {p}: {e}")

print(f'Done. Total fixed: {count}')
