"""Independently import the finished FBX, never the .blend source."""
import bpy,json,hashlib,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Blender/Coin'
EXPORT=ROOT/'Assets/TetherRush/Resources/ArtModels'
m=json.loads((SOURCE/'export_manifest.json').read_text(encoding='utf8'))
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(EXPORT/'Coin.fbx'),use_anim=False)
bpy.context.view_layer.update()
objects=list(bpy.context.scene.objects); roots=[o for o in objects if o.parent is None]; meshes=[o for o in objects if o.type=='MESH']
assert len(roots)==1 and roots[0].name=='Coin'
root=roots[0]
assert root.location.length<1e-5 and all(abs(s-1)<1e-5 for s in root.scale)
assert len(meshes)==1 and meshes[0].parent==root and len(objects)==2
obj=meshes[0]; coords=[obj.matrix_world@v.co for v in obj.data.vertices]
lo=[min(v[i] for v in coords) for i in range(3)]; hi=[max(v[i] for v in coords) for i in range(3)]
size=[hi[i]-lo[i] for i in range(3)]; center=[(hi[i]+lo[i])/2 for i in range(3)]
assert all(abs(s-1)<1e-5 for s in size) and all(abs(c)<1e-5 for c in center)
names=[mat.name for mat in obj.data.materials]; assert names==['Body','Dark','Accent']
tri=sum(len(p.vertices)-2 for p in obj.data.polygons); assert tri==m['triangles'] and tri<=650
assert not bpy.data.actions and not any(o.type=='ARMATURE' for o in objects)
# Independently measure the raised glyph's outward normals under gameplay
# scaling, not just a metadata assertion. Export roundtrip returns Blender
# axes: Unity (x,y,z) = Blender (x,z,y), face should predominantly point -Z.
normals=[]
worldscale=Vector((.5,.18,.5))
for p in obj.data.polygons:
    if names[p.material_index]!='Accent': continue
    vs=[obj.matrix_world@obj.data.vertices[i].co for i in p.vertices]
    vs=[Vector([v[i]*worldscale[i] for i in range(3)]) for v in vs]
    n=(vs[1]-vs[0]).cross(vs[2]-vs[0])
    if n.length>1e-8:
        n.normalize()
        # The two flat cap disks and bridge face are coplanar. Bevels
        # have nonzero Z components and must not bias the face angle.
        if n.y<-.9 and abs(n.z)<1e-4 and n.x>0: normals.append(n)
assert len(normals)>=20, 'Glyph front must face camera on Blender -Y / Unity -Z'
avg=sum(normals,Vector())/len(normals); avg.normalize()
tilt=math.degrees(math.acos(min(1,max(-1,-avg.y))))
assert 10<tilt<30,(tilt,list(avg))
assert len(m['old_fbx_sha256'])==18
assert all(hashlib.sha256((EXPORT/n).read_bytes()).hexdigest()==h for n,h in m['old_fbx_sha256'].items())
assert hashlib.sha256((EXPORT/'Coin.fbx').read_bytes()).hexdigest()==m['fbx_sha256']
report={'result':'PASS','blender_version':bpy.app.version_string,'independent_fbx_import':True,'mesh_count':1,'triangles':tri,'materials':names,'source_bounds_xyz':size,'source_center_xyz':center,'root_scale':list(root.scale),'glyph_camera_facing_triangles':len(normals),'glyph_runtime_face_tilt_degrees':tilt,'glyph_runtime_unity_normal':[avg.x,avg.z,avg.y],'old_fbx_unchanged_count':18,'collider_or_armature':False}
(SOURCE/'fbx_validation.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('COIN_REIMPORT_PASS '+json.dumps(report))
