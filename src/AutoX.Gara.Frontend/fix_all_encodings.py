
import os

# Comprehensive mapping for Mojibake and corruption
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
    "vu?t": "vượt",
    "du?c": "được",
    "kÃ½ t?": "ký tự",
    "ph? tng": "phụ tùng",
    "m?t ph? tng": "một phụ tùng",
    "d? li?u": "dữ liệu",
    "thA'ng tin": "thông tin",
    "nhy cm": "nhạy cảm",
    "tAi khon": "tài khoản",
    "m-t khcu": "mật khẩu",
    "xAc thc": "xác thực",
    "dAnh cho": "dành cho",
    "ng?i dA1ng": "người dùng",
    "h th`ng": "hệ thống",
    "t`i thiu": "tối thiểu",
    "`ng nh-p": "đăng nhập",
    "mA client": "mà client",
    "g-i lAn": "gửi lên",
    "KhA'ng lu": "Không lưu",
    "bt k3": "bất kỳ",
    "bo m-t": "bảo mật",
    "nhy cm": "nhạy cảm",
    "nAo ngoAi": "nào ngoài",
    "tAi khon": "tài khoản",
    "m-t khcu": "mật khẩu",
    "dng clear text": "dạng clear text",
    "ch% `": "chỉ để",
    "xAc thc mTt l n": "xác thực một lần",
}

def fix_file(path):
    try:
        with open(path, 'rb') as f:
            raw = f.read()
        
        # Try different encodings to decode the file
        content = None
        for enc in ['utf-8-sig', 'utf-8', 'latin-1', 'cp1258']:
            try:
                content = raw.decode(enc)
                break
            except:
                continue
        
        if content is None:
            return False

        original = content
        
        # Apply the fix map
        for old, new in fix_map.items():
            content = content.replace(old, new)
        
        is_cs = path.endswith('.cs')
        
        # Determine the correct encoding for saving
        # .cs: UTF-8 with BOM (utf-8-sig)
        # .xaml: UTF-8 without BOM (utf-8)
        target_enc = 'utf-8-sig' if is_cs else 'utf-8'
        
        if content != original:
            with open(path, 'w', encoding=target_enc) as f:
                f.write(content)
            return True
        else:
            # Even if content is the same, ensure the file is saved with the correct BOM/encoding
            # Check if current file has BOM?
            has_bom = raw.startswith(b'\xef\xbb\xbf')
            should_have_bom = is_cs
            
            if has_bom != should_have_bom:
                with open(path, 'w', encoding=target_enc) as f:
                    f.write(content)
                return True
                
    except Exception as e:
        print(f"Error processing {path}: {e}")
    return False

root_dir = r'e:\Cs\AutoX.Gara\src'
total_fixed = 0
for root, dirs, files in os.walk(root_dir):
    # Exclude common noise directories
    for d in ['bin', 'obj', '.vs', '.git', 'Properties']:
        if d in dirs:
            dirs.remove(d)
            
    for file in files:
        if file.endswith(('.cs', '.xaml')):
            path = os.path.join(root, file)
            if fix_file(path):
                print(f"Fixed: {path}")
                total_fixed += 1

print(f"Finished. Total files fixed: {total_fixed}")
