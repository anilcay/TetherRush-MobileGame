"""Render the saved source using the same contact-sheet setup, without changing exports."""
import bpy,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Blender'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'TetherRushKit.blend'))
scene=bpy.context.scene
mats={name:bpy.data.materials[name] for name in ['Body','Dark','Accent']}
assets={}
for collection in bpy.data.collections:
    if collection.name=='Collection': continue
    assets[collection.name]={'meshes':[o for o in collection.objects if o.type=='MESH']}
setup=(ROOT/'Tools/Art/build_blender_kit.py').read_text(encoding='utf-8').split('# Temporary render-only duplicates;')[1]
exec('# Temporary render-only duplicates;'+setup.split("print('M2_ASSET_BUILD_COMPLETE")[0])
