"""Original Tether Rush token. Run using Blender --background --python."""
import bpy, math, json, hashlib
from pathlib import Path
from mathutils import Vector, Matrix

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource/Blender/Coin'
EXPORT=ROOT/'Assets/TetherRush/Resources/ArtModels'
SOURCE.mkdir(parents=True,exist_ok=True)
old={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in EXPORT.glob('*.fbx') if p.stem!='Coin'}
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version=0
scene=bpy.context.scene
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
mats={}
for name,col,metal,rough in [('Body',(1,.57,.12,1),.55,.28),('Dark',(.026,.046,.075,1),.15,.42),('Accent',(1,.84,.46,1),.35,.25)]:
    mat=bpy.data.materials.new(name); mat.diffuse_color=col; mat.use_nodes=True
    p=mat.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=col
    p.inputs['Metallic'].default_value=metal; p.inputs['Roughness'].default_value=rough
    mats[name]=mat
verts=[]; faces=[]; roles=[]; smooth=[]

def surface(rings,n,role,caps=True):
    """Lathe about Blender Y; rings are (radius, depth)."""
    start=len(verts)
    for r,y in rings:
        for j in range(n):
            a=2*math.pi*j/n
            verts.append((r*math.cos(a),y,r*math.sin(a)))
    for k in range(len(rings)-1):
        for j in range(n):
            faces.append((start+k*n+j,start+k*n+(j+1)%n,start+(k+1)*n+(j+1)%n,start+(k+1)*n+j)); roles.append(role); smooth.append(True)
    if caps:
        faces.extend([tuple(start+j for j in reversed(range(n))),tuple(start+(len(rings)-1)*n+j for j in range(n))]); roles.extend([role,role]); smooth.extend([False,False])

# Broad uninterrupted bezel, six rings: rear bevel, edge band, front bevel,
# broad crown and recessed inner return. No microscopic engraved noise.
surface([(.455,.060),(.493,.043),(.500,.020),(.500,-.026),(.466,-.065),(.355,-.065),(.338,-.041)],24,'Body',False)
dark_start=len(faces)
surface([(.338,-.041),(.338,.042)],24,'Dark')
for i in range(dark_start,len(faces)): faces[i]=tuple(reversed(faces[i]))
# Reverse dark face with the same medallion silhouette, front has the emblem.
surface([(.355,.042),(.455,.060)],24,'Body',False)

def stud(x):
    base=len(verts)
    surface([(.105,-.042),(.116,-.056),(.116,-.080),(.098,-.096)],12,'Accent')
    for i in range(base,len(verts)):
        v=verts[i]; verts[i]=(v[0]+x,v[1],v[2])

# A simple two-core + tether pictogram. Bar is broad enough at 22 px.
base=len(verts)
verts.extend([(-.16,-.044,-.042),(.16,-.044,-.042),(.16,-.044,.042),(-.16,-.044,.042),(-.16,-.083,-.042),(.16,-.083,-.042),(.16,-.083,.042),(-.16,-.083,.042)])
for f in [(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]:
    faces.append(tuple(base+i for i in f)); roles.append('Accent'); smooth.append(False)
stud(-.164); stud(.164)
mesh=bpy.data.meshes.new('CoinGeometry'); mesh.from_pydata(verts,[],faces); mesh.update()
obj=bpy.data.objects.new('CoinMesh',mesh); scene.collection.objects.link(obj)
for name in ['Body','Dark','Accent']: mesh.materials.append(mats[name])
for p,role,sm in zip(mesh.polygons,roles,smooth): p.material_index=['Body','Dark','Accent'].index(role); p.use_smooth=sm
# Author in real coin proportions, then retain a clean orthographic icon copy.
icon=obj.copy(); icon.data=obj.data.copy(); scene.collection.objects.link(icon); icon.name='IconRenderOnly'
# Bake a modest yaw to show one edge in the game. Runtime scales source axes
# (X,Y,Z) by (.5,.18,.5), corresponding to Unity (.5,.5,.18).
rot=Matrix.Rotation(math.radians(25),4,'Z')
for v in mesh.vertices: v.co=rot@v.co
lo=Vector([min(v.co[i] for v in mesh.vertices) for i in range(3)])
hi=Vector([max(v.co[i] for v in mesh.vertices) for i in range(3)])
center=(lo+hi)/2; extent=hi-lo
for v in mesh.vertices: v.co=Vector([(v.co[i]-center[i])/extent[i] for i in range(3)])
tri=obj.modifiers.new('Export triangles','TRIANGULATE')
bpy.context.view_layer.objects.active=obj; obj.select_set(True)
bpy.ops.object.modifier_apply(modifier=tri.name)
root=bpy.data.objects.new('Coin',None); scene.collection.objects.link(root); obj.parent=root
icon.hide_render=True; icon.hide_set(True)
bpy.ops.object.select_all(action='DESELECT'); root.select_set(True); obj.select_set(True)
bpy.context.view_layer.objects.active=root
profile=dict(use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',global_scale=1.,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',use_space_transform=True,bake_space_transform=False,bake_anim=False,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False)
bpy.ops.export_scene.fbx(filepath=str(EXPORT/'Coin.fbx'),**profile)
assert len(mesh.polygons)<=900
assert all(hashlib.sha256((EXPORT/n).read_bytes()).hexdigest()==s for n,s in old.items())
manifest={'blender_version':bpy.app.version_string,'id':'Coin','triangles':len(mesh.polygons),'materials':['Body','Dark','Accent'],'bounds_source_xyz':[1,1,1],'root_scale':[1,1,1],'center':[0,0,0],'source_face_normal':'-Y','unity_face_normal':'-Z','source_yaw_degrees':25,'runtime_unity_size':[.5,.5,.18],'old_fbx_sha256':old,'fbx_sha256':hashlib.sha256((EXPORT/'Coin.fbx').read_bytes()).hexdigest()}
(SOURCE/'export_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')

# Render genuine geometry with neutral broad studio lighting.
scene.render.engine='CYCLES'; scene.cycles.samples=32
scene.render.film_transparent=True
scene.world=bpy.data.worlds.new('CoinStudio'); scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.30,.35,.43,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.6
scene.view_settings.view_transform='Standard'
def area(name,loc,power,size):
    d=bpy.data.lights.new(name,'AREA'); d.energy=power; d.shape='DISK'; d.size=size
    o=bpy.data.objects.new(name,d); scene.collection.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
area('Key',(-2,-3,4),260,4); area('Fill',(3,-1,1),90,3)
camdata=bpy.data.cameras.new('CoinCamera'); cam=bpy.data.objects.new('CoinCamera',camdata); scene.collection.objects.link(cam)
scene.camera=cam; camdata.type='ORTHO'
def camera(loc,scale):
    cam.location=loc; cam.rotation_euler=(-cam.location).to_track_quat('-Z','Y').to_euler(); camdata.ortho_scale=scale
scene.render.resolution_percentage=100; scene.render.image_settings.file_format='PNG'; scene.render.image_settings.color_mode='RGBA'
obj.hide_render=True; icon.hide_render=False; icon.hide_set(False)
camera((0,-4,0),1.11)
scene.render.resolution_x=256; scene.render.resolution_y=256
scene.render.filepath=str(SOURCE/'CoinIcon.png'); bpy.ops.render.render(write_still=True)
# Source keeps normalized export and the explicitly named icon-only geometry.
icon.hide_render=True; icon.hide_set(True); obj.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Coin.blend'))
obj.scale=(1,.36,1)
camera((0,-4,0),1.30)
scene.render.resolution_x=768; scene.render.resolution_y=768
scene.render.filepath=str(SOURCE/'CoinWorld.png'); bpy.ops.render.render(write_still=True)
camera((2.6,-4,1.4),1.40)
scene.render.filepath=str(SOURCE/'CoinAngle.png'); bpy.ops.render.render(write_still=True)
camera((4,-1,0),1.40)
scene.render.filepath=str(SOURCE/'CoinEdge.png'); bpy.ops.render.render(write_still=True)
print('COIN_BUILD_PASS '+json.dumps({'triangles':manifest['triangles'],'old_fbx_count':len(old)}))
