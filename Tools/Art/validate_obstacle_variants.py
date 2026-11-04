"""Independent FBX import and contact-slab occupancy checks. No Unity invoked."""
import bpy, json, hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Blender/ObstacleVariants'
EXPORT=ROOT/'Assets/TetherRush/Resources/ArtModels'
manifest=json.loads((SOURCE/'export_manifest.json').read_text(encoding='utf-8'))
report={'blender_version':bpy.app.version_string,'checks':[]}
for asset in manifest['assets']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(EXPORT/(asset['id']+'.fbx')),use_anim=False)
    bpy.context.view_layer.update()
    objects=list(bpy.context.scene.objects); meshes=[o for o in objects if o.type=='MESH']
    roots=[o for o in objects if o.parent is None]
    assert len(roots)==1 and roots[0].name==asset['id']
    root=roots[0]; assert root.location.length<1e-4 and all(abs(s-1)<1e-4 for s in root.scale)
    assert len(meshes)==1 and meshes[0].parent==root
    obj=meshes[0]; coords=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    lo=[min(p[i] for p in coords) for i in range(3)]; hi=[max(p[i] for p in coords) for i in range(3)]
    bounds=[hi[i]-lo[i] for i in range(3)]; center=[(hi[i]+lo[i])/2 for i in range(3)]
    assert all(abs(v-1)<1e-4 for v in bounds) and all(abs(v)<1e-4 for v in center)
    tri=sum(len(p.vertices)-2 for p in obj.data.polygons)
    assert tri==asset['triangles'] and tri<=1000
    names=[m.name for m in obj.data.materials]; assert set(names)=={'Body','Dark','Accent'} and len(names)==3
    assert not bpy.data.actions and not any(o.type=='ARMATURE' for o in objects)
    # Imported world stays Blender Z-up. Raycast both horizontal directions at
    # several contact heights, including points .0001 inside all four corners.
    # This explicitly catches an empty X/cross or chamfered contact footprint.
    inv=obj.matrix_world.inverted(); samples=0
    for z in [-.49,-.10,0,.10,.15]:
        for transverse in [-.4999,-.4,0,.4,.4999]:
            for axis in [0,1]:
                start=Vector((-1,transverse,z) if axis==0 else (transverse,-1,z))
                direction=Vector((1,0,0) if axis==0 else (0,1,0))
                hit,location,normal,face=obj.ray_cast(inv@start,inv.to_3x3()@direction)
                assert hit,(asset['id'],start)
                world=obj.matrix_world@location
                assert abs(world[axis]+.5)<1e-4,(asset['id'],list(world),axis)
                samples+=1
    report['checks'].append({'id':asset['id'],'result':'PASS','triangles':tri,'mesh_count':1,'material_slots':names,'imported_bounds_xyz':bounds,'center':center,'root_scale':list(root.scale),'contact_front_edge_rays':samples})
assert all(hashlib.sha256((EXPORT/name).read_bytes()).hexdigest()==sha for name,sha in manifest['old_fbx_sha256'].items())
report['old_fbx_unchanged_count']=len(manifest['old_fbx_sha256'])
(SOURCE/'fbx_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('OBSTACLE_VARIANTS_REIMPORT_PASS '+str(len(report['checks'])))
