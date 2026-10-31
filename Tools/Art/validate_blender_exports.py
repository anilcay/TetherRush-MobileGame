"""Independent FBX re-import smoke validation; Unity import remains a separate acceptance."""
import bpy,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
source=ROOT/'ArtSource/Blender'
manifest=json.loads((source/'export_manifest.json').read_text(encoding='utf-8'))
report={'blender_version':bpy.app.version_string,'checks':[]}
for asset in manifest['assets']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(ROOT/'Assets/TetherRush/Resources/ArtModels'/(asset['id']+'.fbx')),use_anim=False)
    bpy.context.view_layer.update()
    objects=list(bpy.context.scene.objects)
    meshes=[o for o in objects if o.type=='MESH']
    coords=[o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
    lo=[min(p[i] for p in coords) for i in range(3)]; hi=[max(p[i] for p in coords) for i in range(3)]
    bounds=[hi[i]-lo[i] for i in range(3)]
    center=[(hi[i]+lo[i])/2 for i in range(3)]
    tri=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
    names=sorted(set(m.name for o in meshes for m in o.data.materials))
    roots=[o for o in objects if o.parent is None]
    assert all(abs(v-1)<.0001 for v in bounds),(asset['id'],bounds)
    assert all(abs(v)<.0001 for v in center),(asset['id'],center)
    assert len(roots)==1 and roots[0].name==asset['id']
    assert roots[0].location.length<.0001
    assert tri==asset['triangles'],(tri,asset['triangles'])
    assert set(names)<= {'Body','Dark','Accent'},names
    children={o.name for o in meshes}
    if asset['id']=='EnergyGate': assert children=={'DoorLeft','DoorRight','Frame'},children
    if asset['id']=='BreakableBlock': assert children=={'Shard1','Shard2','Shard3','Shard4'},children
    if asset['id']=='TrackTile':
        assert children=={'Deck','TrackTrims'},children
        deck=next(o for o in meshes if o.name=='Deck')
        # Blender's imported FBX world remains Z-up; Unity maps that vertical to Y.
        top=max((deck.matrix_world @ Vector(c)).z for c in deck.bound_box)
        assert abs(top-asset['deck_top_normalized_unity_y'])<.0001,top
    assert not bpy.data.actions and not any(o.type=='ARMATURE' for o in objects)
    report['checks'].append({'id':asset['id'],'imported_bounds_xyz':bounds,'bounds_center':center,'triangles':tri,'mesh_count':len(meshes),'root':roots[0].name,'materials':names,'result':'PASS'})
(source/'fbx_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('FBX_REIMPORT_PASS '+str(len(report['checks'])))
