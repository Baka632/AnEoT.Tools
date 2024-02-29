import argparse
import os;
from lanzou.api import LanZouCloud
import markdown_writer;

def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="Download page updater for AnEoT", description="一个自动将 PDF 文件上传到指定位置，并自动更新 download.md 的程序")
    parser.add_argument("--lanzou-ylogin", type=str, required=True, help="用于蓝奏云网盘登陆，此项位于 woozooo.com -> Cookie -> ylogin")
    parser.add_argument("--lanzou-phpdisk-info", type=str, required=True, help="用于蓝奏云网盘登陆，此项位于 pc.woozooo.com -> Cookie -> phpdisk_info")
    parser.add_argument("--lanzou-target-folder-path", type=str, help="蓝奏云网盘中，用于保存上传文件的文件夹目标路径，这应当是一个相对路径")
    parser.add_argument("--lanzou-share-folder-password", type=str, required=True, help="蓝奏云共享文件夹的密码")
    parser.add_argument("--single-page-file-name", type=str, required=True, help="单页文件名，此参数不涉及到蓝奏云网盘，只用于写入到 download.md 文件中")
    parser.add_argument("--folder-path", type=str, required=True, help="要上传到蓝奏云网盘的文件夹路径")
    parser.add_argument("--issue-title", type=str, required=True, help="期刊名称，此参数不涉及到蓝奏云网盘，只用于写入到 download.md 文件中")
    parser.add_argument("--download-md-path", type=str, required=True, help="download.md 文件的路径")
    return parser

def show_failed(code, filename):
    """显示失败文件的回调函数"""
    print(f"上传失败,错误码: {code}, 文件名: {filename}")

def show_progress(file_name, total_size, now_size):
    """显示进度的回调函数"""
    percent = now_size / total_size
    bar_len = 40  # 进度条长总度
    bar_str = '>' * round(bar_len * percent) + '=' * round(bar_len * (1 - percent))
    print('\r{:.2f}%\t[{}] {:.1f}/{:.1f}MB | {} '.format(
        percent * 100, bar_str, now_size / 1048576, total_size / 1048576, file_name), end='')
    if total_size == now_size:
        print('')  # 下载完成换行

def upload_files_from_folder(dir_path, dir_id, lanzouClient: LanZouCloud):
    for filename in os.listdir(dir_path):
            file_path = dir_path + os.sep + filename
            if not os.path.isfile(file_path):
                continue  # 跳过子文件夹
            code = lanzouClient.upload_file(file_path, dir_id, callback=show_progress)
            if code != LanZouCloud.SUCCESS:
                show_failed(code, filename)

if  __name__ == "__main__":
    print("请稍后...")
    parser = create_parser()
    args = parser.parse_args()
    
    lzy = LanZouCloud()
    cookie = {'ylogin': args.lanzou_ylogin, 'phpdisk_info': args.lanzou_phpdisk_info}
    login_result = lzy.login_by_cookie(cookie)
    if login_result == LanZouCloud.FAILED:
        print("蓝奏云网盘登陆失败，请检查你输入的 'ylogin' 和 'phpdisk_info' 是否正确")
        print("'ylogin' 位于 woozooo.com -> Cookie -> ylogin")
        print("'phpdisk_info' 位于 pc.woozooo.com -> Cookie -> phpdisk_info")
        exit(1)
    elif login_result == LanZouCloud.NETWORK_ERROR:
        print("蓝奏云网盘登陆失败，请检查你的网络链接")
        exit(1)
    
    parent_id = -1;
    for string in args.lanzou_target_folder_path.replace("\\", "/").split("/"):
        if len(string.strip()) != 0:
            result = lzy.mkdir(parent_id, string)
            if result == LanZouCloud.MKDIR_ERROR:
                print(f"尝试创建文件夹 '{string}' 时失败");
                exit(1)
            else:
                parent_id = result
    upload_files_from_folder(args.folder_path, parent_id, lzy)
    if lzy.set_passwd(parent_id, args.lanzou_share_folder_password, False) != LanZouCloud.SUCCESS:
        print("警告：设置自定义文件夹密码失败")
    share_info = lzy.get_share_info(parent_id, False)
    print(f"上传信息：\n文件夹名：{share_info.name}\n共享链接：{share_info.url}\n密码：{share_info.pwd}")
    if share_info.pwd != args.lanzou_share_folder_password:
        print("警告：实际的共享密码与预先指定的密码不同")
        print(f"预先指定的密码为：{args.lanzou_share_folder_password}")
        print(f"实际的共享密码为：{share_info.pwd}")
    markdown_writer.appendDownloadTableToFile(args.download_md_path, args.issue_title, args.single_page_file_name, share_info.url, share_info.pwd);
    input("请按回车键继续...")