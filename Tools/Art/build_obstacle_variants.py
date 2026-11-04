"""Reproducible, isolated obstacle kit. Blender --background --python this_file."""
import bpy, json, math, hashlib
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Blender/ObstacleVariants'
EXPORT = ROOT / 'Assets/TetherRush/Resources/ArtModels'
SOURCE.mkdir(parents=True, exist_ok=True)
IDS = ['ClampBarrier', 'CapacitorBank', 'HydraulicSlider', 'TurbineBeam']
old_hashes = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in EXPORT.glob('*.fbx') if p.stem not in IDS}
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
scene = bpy.context.scene
scene.world = bpy.data.worlds.new('ObstacleVariantsWorld')
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
mats = {}
for name, col in {'Body':(.25,.32,.39,1), 'Dark':(.06,.10,.15,1), 'Accent':(.83,.40,.44,1)}.items():
    mat = bpy.data.materials.new(name); mat.diffuse_color = col
    mats[name] = mat
assets = {}
parts = []

def box(name, center, size, role='Body', bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center)
    obj = bpy.context.object; obj.name = name; obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mats[role])
    if bevel:
        mod = obj.modifiers.new('Machined bevel', 'BEVEL'); mod.width=bevel; mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    parts.append(obj)
    return obj

def cylinder(name, center, radius, depth, role, vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=center)
    obj=bpy.context.object; obj.name=name; obj.data.materials.append(mats[role]); parts.append(obj)
    return obj

def shoulder(name, width=.76, depth=.66, slope=0, top=.5):
    # Full rectangular collision footprint ends at +.14. All shoulder
    # chamfers are above that plane. No disconnected contact slab.
    def contact(z):
        return [(-.46,-.5,z),(.46,-.5,z),(.5,-.5,z),(.5,.5,z),
                (.46,.5,z),(-.46,.5,z),(-.5,.5,z),(-.5,-.5,z)]
    def upper(w,d,c,z):
        xy=[(-w/2+c,-d/2),(w/2-c,-d/2),(w/2,-d/2+c),(w/2,d/2-c),
            (w/2-c,d/2),(-w/2+c,d/2),(-w/2,d/2-c),(-w/2,-d/2+c)]
        return [(x,y,z+slope*y) for x,y in xy]
    rings=[contact(-.5),contact(.14),upper(.98,.98,.09,.24),
           upper(width,depth,.09,top-abs(slope)*depth/2)]
    verts=[v for ring in rings for v in ring]
    faces=[tuple(reversed(range(8)))]
    for k in range(3):
        for j in range(8): faces.append((k*8+j,k*8+(j+1)%8,(k+1)*8+(j+1)%8,(k+1)*8+j))
    faces.append(tuple(range(24,32)))
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(verts,[],faces); mesh.update()
    obj=bpy.data.objects.new(name,mesh); scene.collection.objects.link(obj)
    obj.data.materials.append(mats['Body']); parts.append(obj)

def begin(name):
    global parts
    parts=[]

def finish(name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join(); obj=parts[0]; obj.name=name+'Mesh'
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    # Joining can repeat material slots. Consolidate to the exact three roles.
    roles=[obj.data.materials[p.material_index].name for p in obj.data.polygons]
    obj.data.materials.clear()
    for role in ['Body','Dark','Accent']: obj.data.materials.append(mats[role])
    for p,role in zip(obj.data.polygons,roles): p.material_index=['Body','Dark','Accent'].index(role)
    mod=obj.modifiers.new('Export triangles','TRIANGULATE'); bpy.ops.object.modifier_apply(modifier=mod.name)
    root=bpy.data.objects.new(name,None); scene.collection.objects.link(root); obj.parent=root
    collection=bpy.data.collections.new(name); scene.collection.children.link(collection)
    for item in [root,obj]:
        for old_collection in list(item.users_collection): old_collection.objects.unlink(item)
        collection.objects.link(item)
    assets[name]=(root,obj)

begin('ClampBarrier')
shoulder('SmoothShoulder',.80,.72,.10)
box('FootRecess',(0,.495,-.46),(.94,.01,.07),'Dark')
box('WarningInlay',(0,-.17,.453),(.60,.045,.012),'Accent',.004)
finish('ClampBarrier')

begin('CapacitorBank')
shoulder('RoundedMonolith',.68,.80,-.18)
box('BaseReveal',(0,.495,-.46),(.94,.01,.07),'Dark')
# Inlay follows wedge surface instead of floating above it.
o=box('LinearWarning',(0,-.30,.488),(.45,.025,.012),'Accent',.003)
o.rotation_euler[0]=math.atan(-.18)
finish('CapacitorBank')

begin('HydraulicSlider')
shoulder('PistonCap',.78,.68,0,top=.49)
box('SlideRecess',(0,.497,-.43),(.80,.006,.10),'Dark')
o=box('DirectionStroke',(0,-.04,.496),(.40,.055,.008),'Accent',.003)
o.rotation_euler[2]=-.38
finish('HydraulicSlider')

begin('TurbineBeam')
shoulder('ContinuousRotor',.92,.64,0,top=.48)
cylinder('SimpleHub',(0,0,.49),.15,.02,'Dark',24)
box('EdgeStripe',(0,-.235,.488),(.82,.045,.012),'Accent',.003)
finish('TurbineBeam')

profile=dict(use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',global_scale=1.,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',use_space_transform=True,bake_space_transform=False,bake_anim=False,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='AUTO')
manifest={'blender_version':bpy.app.version_string,'profile':{k:sorted(v) if isinstance(v,set) else v for k,v in profile.items()},'old_fbx_sha256':old_hashes,'assets':[]}
for name,(root,obj) in assets.items():
    bpy.context.view_layer.update()
    coords=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    lo=[min(p[i] for p in coords) for i in range(3)]; hi=[max(p[i] for p in coords) for i in range(3)]
    assert all(abs(lo[i]+.5)<1e-5 and abs(hi[i]-.5)<1e-5 for i in range(3)),(name,lo,hi)
    tri=len(obj.data.polygons); assert tri<=1000,(name,tri)
    bpy.ops.object.select_all(action='DESELECT'); root.select_set(True); obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    dest=EXPORT/(name+'.fbx'); bpy.ops.export_scene.fbx(filepath=str(dest),**profile)
    manifest['assets'].append({'id':name,'triangles':tri,'mesh_count':1,'material_slots':['Body','Dark','Accent'],'bounds_source_xyz':[1,1,1],'center':[0,0,0],'root_scale':[1,1,1],'contact_slab_source_z':[-.5,.14],'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'bytes':dest.stat().st_size})
assert all(hashlib.sha256((EXPORT/name).read_bytes()).hexdigest()==sha for name,sha in old_hashes.items())
(SOURCE/'export_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
for name in assets:
    bpy.data.collections[name].hide_viewport=name!='ClampBarrier'
    bpy.data.collections[name].hide_render=True
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'ObstacleVariants.blend'))

# Preview duplicates only: source save and FBX remain in local normalized space.
for root,obj in assets.values(): obj.hide_render=True
for i,(name,(root,obj)) in enumerate(assets.items()):
    x=(i%2-.5)*3.8; y=(.5-i//2)*3.4
    dup=obj.copy(); dup.data=obj.data.copy(); dup.parent=None; scene.collection.objects.link(dup)
    dup.hide_render=False; dup.location=(x,y,0)
    dup.rotation_euler=(math.radians(-48),0,math.radians(-18))
    dup.scale={'ClampBarrier':(2.2,1.2,.9),'CapacitorBank':(2.35,1.0,.9),'HydraulicSlider':(1.15,1.2,.9),'TurbineBeam':(4.2,.72,.9)}[name]
    curve=bpy.data.curves.new(name+'Label','FONT'); curve.body=name; curve.align_x='CENTER'; curve.size=.23
    label=bpy.data.objects.new(name+'Label',curve); scene.collection.objects.link(label); label.location=(x,y-1.25,.4); curve.materials.append(mats['Body'])
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'; scene.display.shading.studio_light='paint.sl'
scene.display.shading.color_type='MATERIAL'; scene.view_settings.view_transform='Standard'; scene.display.shading.show_shadows=False
scene.display.shading.show_cavity=False; scene.display.shading.cavity_type='BOTH'
scene.display.shading.background_type='WORLD'; scene.world.color=(.018,.025,.04)
camdata=bpy.data.cameras.new('PreviewCamera'); cam=bpy.data.objects.new('PreviewCamera',camdata); scene.collection.objects.link(cam)
cam.location=(0,0,15); camdata.type='ORTHO'; camdata.ortho_scale=8.4; scene.camera=cam
scene.render.resolution_x=1800; scene.render.resolution_y=1600; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'; scene.render.filepath=str(SOURCE/'preview.png')
bpy.ops.render.render(write_still=True)
print('OBSTACLE_VARIANTS_BUILD_PASS '+json.dumps(manifest))
