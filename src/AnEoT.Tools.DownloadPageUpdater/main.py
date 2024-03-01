import argparse
import os;
from lanzou.api import LanZouCloud
import markdown_writer;
import json

def parse_config(cfg_path: str) -> tuple[str, str]:
    try:
        with open(cfg_path, encoding="utf-8") as cfg_file:
            cfg_str = cfg_file.read()
            obj = json.loads(cfg_str)
            if type(obj) is dict:
                cfg_dict: dict = obj
                ylogin = cfg_dict.get("ylogin")
                phpdisk_info = cfg_dict.get("phpdisk_info")
                if phpdisk_info is not None and ylogin is not None:
                    return (ylogin, phpdisk_info)
        return ("", "")
    except:
        return ("", "")

def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(epilog="Download page updater for AnEoT\nEureka, my beloved ——Baka632", description="一个自动将 PDF 文件上传到指定位置，并自动更新 download.md 的程序")
    
    no_config_group = parser.add_argument_group("不带配置文件")
    no_config_group.add_argument("--lanzou-ylogin", default="", type=str, help="用于蓝奏云网盘登陆，此项位于 woozooo.com -> Cookie -> ylogin")
    no_config_group.add_argument("--lanzou-phpdisk-info", default="", type=str, help="用于蓝奏云网盘登陆，此项位于 pc.woozooo.com -> Cookie -> phpdisk_info")
    
    config_group = parser.add_argument_group("带配置文件")
    config_path_help = """
    配置文件路径，其格式如下：
    {
        "ylogin": "***",
        "phpdisk_info": "***"
    }
"""
    config_group.add_argument("--config-path", default="", type=str, help=config_path_help)

    optional_group = parser.add_argument_group("可选参数")
    optional_group.add_argument("--lanzou-target-folder-path", type=str, help="蓝奏云网盘中，用于保存上传文件的文件夹目标路径，这应当是一个相对路径", default="/Upload")
    optional_group.add_argument("--lanzou-share-password", default="", type=str, help="蓝奏云共享文件夹的密码")
    
    required_group = parser.add_argument_group("必选参数")
    required_group.add_argument("--single-page-file-name", type=str, required=True, help="单页 PDF 的文件名，此参数不涉及到蓝奏云网盘，只用于写入到 download.md 文件中")
    required_group.add_argument("--folder-path", type=str, required=True, help="要上传到蓝奏云网盘的本地文件夹路径")
    required_group.add_argument("--issue-title", type=str, required=True, help="期刊名称，此参数不涉及到蓝奏云网盘，只用于写入到 download.md 文件中")
    required_group.add_argument("--download-md-path", type=str, required=True, help="download.md 文件的路径")
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
    
    lanzou_ylogin = ""
    lanzou_phpdisk_info = ""

    if len(args.config_path.strip()) != 0:
        lanzou_ylogin, lanzou_phpdisk_info = parse_config(args.config_path)

    if len(args.lanzou_ylogin.strip()) != 0:
        lanzou_ylogin = args.lanzou_ylogin

    if len(args.lanzou_phpdisk_info.strip()) != 0:
        lanzou_phpdisk_info = args.lanzou_phpdisk_info

    if len(lanzou_ylogin.strip()) == 0 or len(lanzou_phpdisk_info.strip()) == 0:
        print("未能读取到有效的 'ylogin' 或 'phpdisk_info'")
        exit(1)

    cookie = {'ylogin': lanzou_ylogin, 'phpdisk_info': lanzou_phpdisk_info}
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
    if len(args.lanzou_share_password.strip()) != 0:
        if lzy.set_passwd(parent_id, args.lanzou_share_password, False) != LanZouCloud.SUCCESS:
            print("警告：设置自定义文件夹密码失败")
    share_info = lzy.get_share_info(parent_id, False)
    print(f"上传信息：\n\t文件夹名：{share_info.name}\n\t共享链接：{share_info.url}\n\t密码：{share_info.pwd}")
    if share_info.pwd != args.lanzou_share_password:
        print("警告：实际的共享密码与预先指定的密码不同")
        print(f"预先指定的密码为：{args.lanzou_share_password}")
        print(f"实际的共享密码为：{share_info.pwd}")
    markdown_writer.appendDownloadTableToFile(args.download_md_path, args.issue_title, args.single_page_file_name, share_info.url, share_info.pwd);
    input("请按回车键继续...")