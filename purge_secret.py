import os
import sys

# The sensitive part of the URL to purge
secret = "REPLACE_WITH_ENV_VAR"
placeholder = "REPLACE_WITH_ENVIRONMENT_VARIABLE"

def purge():
    for root, dirs, files in os.walk('.'):
        if '.git' in root:
            continue
        for file in files:
            if file.endswith(('.ts', '.js', '.map', '.md', '.txt')):
                path = os.path.join(root, file)
                try:
                    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
                        content = f.read()
                    if secret in content:
                        print(f"Purging secret from {path}")
                        new_content = content.replace(secret, placeholder)
                        with open(path, 'w', encoding='utf-8') as f:
                            f.write(new_content)
                except Exception as e:
                    print(f"Error processing {path}: {e}")

if __name__ == "__main__":
    purge()
