import os
import json
import shutil
import urllib.parse
import platform

# ==========================================
# CẤU HÌNH TÊN THƯ MỤC DỰ ÁN CỦA BẠN
# Thay "ten_thu_muc_du_an" bằng tên thư mục gốc chứa code bị mất của bạn
PROJECT_NAME = "EducationSystem"
# ==========================================

is_windows = platform.system() == 'Windows'

if is_windows:
    history_dir = os.path.join(os.environ.get('APPDATA', ''), 'Code', 'User', 'History')
elif platform.system() == 'Darwin':
    history_dir = os.path.expanduser('~/Library/Application Support/Code/User/History')
else:
    history_dir = os.path.expanduser('~/.config/Code/User/History')

dest_dir = os.path.join(os.getcwd(), 'CodeDaCuu')
if not os.path.exists(dest_dir):
    os.makedirs(dest_dir)

print("Đang quét dữ liệu lịch sử của VS Code...")
recovered_count = 0

if not os.path.exists(history_dir):
    print("❌ Không tìm thấy thư mục History của VS Code!")
    exit()

for folder in os.listdir(history_dir):
    folder_path = os.path.join(history_dir, folder)
    entries_path = os.path.join(folder_path, 'entries.json')
    
    if os.path.isfile(entries_path):
        try:
            with open(entries_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                
            resource = urllib.parse.unquote(data.get('resource', ''))
            
            # Kiểm tra xem file này có thuộc dự án của bạn không
            if PROJECT_NAME in resource:
                entries = data.get('entries', [])
                if entries:
                    # Lấy bản backup cuối cùng (mới nhất)
                    latest_entry = entries[-1]
                    latest_backup_path = os.path.join(folder_path, latest_entry.get('id', ''))
                    
                    if os.path.exists(latest_backup_path):
                        # Cắt đường dẫn để giữ nguyên cấu trúc thư mục
                        parts = resource.split(PROJECT_NAME + '/')
                        if len(parts) > 1:
                            relative_path = parts[1]
                        else:
                            relative_path = os.path.basename(resource)
                            
                        # Xử lý chuẩn hóa dấu gạch chéo
                        relative_path = relative_path.replace('\\', '/')
                        final_dest_path = os.path.join(dest_dir, relative_path)
                        
                        # Tạo thư mục cha nếu chưa có và copy file
                        os.makedirs(os.path.dirname(final_dest_path), exist_ok=True)
                        shutil.copy2(latest_backup_path, final_dest_path)
                        recovered_count += 1
        except Exception:
            pass

print(f"\n🎉 Hoàn thành! Đã khôi phục thành công {recovered_count} files.")
print(f"👉 Bạn hãy kiểm tra thư mục 'CodeDaCuu' nằm ngay cạnh file script này.")