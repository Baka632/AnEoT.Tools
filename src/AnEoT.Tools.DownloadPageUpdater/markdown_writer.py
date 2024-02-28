# 字符串中使用的的占位符：
# issueTitle：期刊名（如“Vol.19 2024年02月号”）
# singlePagePdfFileName：单页版本的 PDF 文件名，此值通常与 GitHub 仓库中的 PDF 文件相对应（如“回归线Vol.19_24-02.pdf”）
# lanzouPassword：蓝奏云的密码（如“202402”）
# lanzouLink：蓝奏云网盘的链接
markdownTableTemplate = """### {issueTitle}

| 文件名称 | 下载地址1 | 下载地址2 | 下载地址3 | 下载地址4 | 网盘下载 |
|:-:|:-:|:-:|:-:|:-:|:-:|
| {issueTitle} | [点击下载](https://cdn.jsdelivr.net/gh/TCA-Arknights/aneot@main/pdf/{singlePagePdfFileName}) | [点击下载](https://aneot.api.wuyilingwei.com/{singlePagePdfFileName}) | [点击下载](https://raw.kgithub.com/TCA-Arknights/aneot/main/pdf/{singlePagePdfFileName}) | [点击下载](https://raw.githubusercontent.com/TCA-Arknights/aneot/main/pdf/{singlePagePdfFileName}) | [密码:{lanzouPassword}]({lanzouLink}) |""";

def appendDownloadTableToFile(path: str, issueTitle: str, singlePagePdfFileName: str, lanzouLink: str, lanzouPassword: str):
    tableString = f"""{markdownTableTemplate.format(issueTitle = issueTitle, singlePagePdfFileName = singlePagePdfFileName, lanzouLink = lanzouLink, lanzouPassword = lanzouPassword)}

"""
    with open(path, "r+", encoding="utf-8") as downloadMarkdownFile:
        markdownLines = downloadMarkdownFile.readlines()
        
        insertPosition = markdownLines.index("## OneDrive Business（备用）\n")
        markdownLines.insert(insertPosition, tableString);
        
        downloadMarkdownFile.seek(0)
        downloadMarkdownFile.writelines(markdownLines);