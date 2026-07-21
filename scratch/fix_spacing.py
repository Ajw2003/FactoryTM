import os
import re

directories = [
    r"c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\Assets\Resources\prefabs\Ui",
    r"c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\Assets\Resources\prefabs\Managers",
    r"c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\Assets\Prefabs"
]

spacing_val = "10"
padding_val = "10"

for directory in directories:
    if not os.path.exists(directory):
        continue
    for root, _, files in os.walk(directory):
        for f in files:
            if f.endswith(".prefab"):
                path = os.path.join(root, f)
                with open(path, 'r', encoding='utf-8', newline='') as file:
                    content = file.read()
                
                blocks = content.split('--- !u!')
                new_blocks = []
                for block in blocks:
                    # If this block is a MonoBehaviour and has m_Spacing (likely a layout group)
                    if 'MonoBehaviour:' in block and 'm_Spacing:' in block and 'm_Padding:' in block:
                        block = re.sub(r'm_Spacing:\s*-?\d+(\.\d+)?', f'm_Spacing: {spacing_val}', block)
                        # inside this block, replace m_Left, m_Right, m_Top, m_Bottom
                        block = re.sub(r'm_Left:\s*-?\d+(\.\d+)?', f'm_Left: {padding_val}', block)
                        block = re.sub(r'm_Right:\s*-?\d+(\.\d+)?', f'm_Right: {padding_val}', block)
                        block = re.sub(r'm_Top:\s*-?\d+(\.\d+)?', f'm_Top: {padding_val}', block)
                        block = re.sub(r'm_Bottom:\s*-?\d+(\.\d+)?', f'm_Bottom: {padding_val}', block)
                    new_blocks.append(block)
                
                new_content = '--- !u!'.join(new_blocks)
                with open(path, 'w', encoding='utf-8', newline='') as file:
                    file.write(new_content)

print("Spacing and padding standardized.")
