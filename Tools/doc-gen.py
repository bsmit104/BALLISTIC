import os

'''
Run with: $ python doc-gen.py
Creates md documentation for C# scripts in Assets/Scripts using summary comments.
'''

FOLDER_PATH = "BALLISTIC/Assets/Scripts"
DOCS_PATH = "Docs"

comments = ["<summary>", "Tooltip"]

def get_cs_files() -> list:
    '''
    Recurse through the Assets/Scripts folder to find all cs files.
    Returns a list of the file name, and the file's path from Scripts.
    '''

    cs_filenames = []
    for root, dirs, files in os.walk(FOLDER_PATH):
        for filename in files:
            if filename.endswith(".cs"):
                cs_filenames.append((filename, root[len(FOLDER_PATH):].replace("\\", "/")))
    
    return cs_filenames

def find_next_summary(file):
    '''
    Returns the next summary comment or tooltip found in the file.
    If the end of the file is reached then None string is returned.
    '''
    # Find the next summary or tooltip
    comment_type = None
    last_line = ""
    for line in file:
        last_line = line
        for comment in comments:
            if comment in line:
                comment_type = comment
                break
        if comment_type is not None:
            break
    
    # Exit if nothing was found by the end of the file
    if comment_type is None:
        return None
    
    if comment_type == "<summary>":
        # Get summary
        summary_str = ""
        for line in file:
            if "/summary" in line:
                break
            summary_str += line.replace("///", "").strip()

        # Get params
        params = []
        for line in file:
            last_line = line
            if "param" not in line:
                break
            # param description
            start_ind = line.find(">")
            end_ind = line[start_ind:].find("<")
            desc = line[start_ind + 1:start_ind + end_ind]

            # param name
            start_ind = line.find("name=") + len("name=") + 1
            end_ind = line[start_ind:].find('"')
            name = line[start_ind:start_ind + end_ind]

            params.append((name, desc))
        
        returns = ""
        if 'returns' in last_line:
            start_ind = line.find(">")
            end_ind = line[start_ind:].find("<")
            returns = line[start_ind + 1:start_ind + end_ind]
            last_line = file.readline()
        
        signature = ""
        sign_type = "method"
        buffer = last_line
        while "RequireComponent" in buffer:
            buffer = file.readline()
        ind = 0
        while ind < len(buffer):
            if buffer[ind] == '{' or buffer[ind] == ';':
                break
            signature += buffer[ind]
            if buffer[ind] == ')':
                break
            elif buffer[ind] == '\n':
                buffer = file.readline()
                ind = 0
            else:
                ind += 1
        signature = signature.replace("  ", "").strip()
        if "class" in signature or "struct" in signature:
            sign_type = "class"
            params = None

        return "summary", (sign_type, signature, summary_str, params, returns)
    
    elif comment_type == "Tooltip":
        start_ind = last_line.find('(')
        end_ind = last_line.find(')')
        tip_str = last_line[start_ind + 2:end_ind - 1]

        last_line = file.readline()
        ind = 0
        while ind < len(last_line):
            if ']' not in last_line:
                break
            if last_line[ind] == ']':
                break
            ind += 1
            if ind >= len(last_line):
                last_line = file.readline()
                ind = 0
        signature = last_line[ind + 1:].replace(';', '').strip()

        return "tip", (tip_str, signature)


def write_summary(file, summary):
    '''
    Formats the given summary and writes it to the given file.
    '''
    if summary[0] == "summary":
        sign_type, signature, summary_str, params, returns = summary[1]
        if sign_type == "method":
            file.write(">> **`" + signature + "`**\\\n")
            file.write(">> " + summary_str + "\n>> \n")
        else:
            file.write("> ## `" + signature + "`\n")
            file.write("> **" + summary_str + "**\n> \n")
            return
        
        if len(params) > 0:
            file.write(">> **Arguments:**\\\n")
        for param in params:
            file.write(f">> *{param[0]}:* {param[1]}")
            if param != params[-1]:
                file.write("\\")
            file.write("\n")
        if returns != "":
            file.write(">>\n>>**Returns:** " + returns + "\n")
        
        if sign_type == "method":
            file.write("> \n")
    
    else:
        tip_str, signature = summary[1]
        file.write(">> **`" + signature + "`**\\\n")
        file.write(">> " + tip_str + "\n> \n")

def build_docs(filenames):
    '''
    Generate md files using summary comments found in each cs file.
    '''
    # Make docs folder
    if not os.path.exists(DOCS_PATH):
        os.makedirs(DOCS_PATH)
    
    # Make glossary to link to all file docs
    with open(DOCS_PATH + "/Glossary.md", "w") as glossary:
        glossary.write("# Code Documentation Glossary\n")

        for filename in filenames:
            glossary.write(f"## [{filename[0]}]({filename[0]}.md)\n")
    
    # Make each file's doc
    for filename in filenames:
        doc = open(DOCS_PATH + "/" + filename[0] + ".md", "w")
        cs_file = open(FOLDER_PATH + filename[1] + "/" + filename[0], "r")

        # Create header for doc
        full_path = "../" + FOLDER_PATH + filename[1] + '/' + filename[0]
        doc.write(f"# {filename[0]}\n**Found in [{filename[1]}]({full_path})**\n\n")
        doc.write(f"[Return to glossary](glossary.md)\n\n")

        # Iterate through each summary and tooltip
        classes = {}
        classes[None] = {"methods": [], "properties": []}
        cur_class = None

        next_summary = find_next_summary(cs_file)
        while next_summary is not None:
            if next_summary[0] == "summary":
                if next_summary[1][0] != "method":
                    classes[next_summary] = {"methods": [], "properties": []}
                    cur_class = next_summary
                else:
                    classes[cur_class]["methods"].append(next_summary)
            else:
                classes[cur_class]["properties"].append(next_summary)
            next_summary = find_next_summary(cs_file)
        
        for cls in classes:
            if cls != None:
                write_summary(doc, cls)
            
            if cls != None and len(classes[cls]["properties"]) > 0:
                doc.write("> ### **Serialized Properties:**\n")
            for prop in classes[cls]["properties"]:
                write_summary(doc, prop)

            if cls != None and len(classes[cls]["methods"]) > 0:
                doc.write("> ### **Methods, Getters, and Setters:**\n")
            for method in classes[cls]["methods"]:
                write_summary(doc, method)
        
        doc.close()
        cs_file.close()

if __name__ == '__main__':
    filenames = get_cs_files()
    for filename in filenames:
        print(filename)
    build_docs(filenames)