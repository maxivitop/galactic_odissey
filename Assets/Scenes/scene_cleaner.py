import re
import os, shutil

scene_name = "./SampleScene.unity"
bac_name = "./SampleScene.unity.bac"

tmp_name = "tmp.unity"

input_file = open(scene_name, "r")
output_file = open(tmp_name, "w")
br = 0
ot = 0

for line in input_file:
    match = re.match(r'  m_IndexBuffer:', line) # Should be your regular expression
    if match:
        line = '  m_IndexBuffer: 0'
    match = re.match(r'    _typelessdata: ', line) # Should be your regular expression
    if match:
        line = '    _typelessdata:  0'

    match = re.match(r'    m_DataSize: ', line) # Should be your regular expression
    if match:
        line = '    m_DataSize: 0'
    output_file.write(line)
input_file.close()
output_file.close()

shutil.copy(scene_name, bac_name)
os.remove(scene_name)
shutil.move(tmp_name, scene_name)
