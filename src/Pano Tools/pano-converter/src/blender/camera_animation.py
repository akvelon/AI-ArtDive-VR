import bpy
from math import *
from mathutils import *

def animate_rotation(x_angle_in_deg, frame_start, frame_end):
    deg_360_in_rad = 2 * pi
    x_angle_in_rad = x_angle_in_deg * deg_360_in_rad / 360
    frames_count = frame_end - frame_start + 1
    
    for n in range(frames_count):
        # calculate angle in radians
        z_angle_in_rad = (n / frames_count) * deg_360_in_rad
        
        # set frame
        bpy.context.scene.frame_set(frame_start + n)
        
        # set current rotation
        myobj.rotation_euler.x = x_angle_in_rad
        myobj.rotation_euler.z = z_angle_in_rad

        # insert new keyframe for "rotation_euler"
        myobj.keyframe_insert(data_path="rotation_euler")

# get camera
myobj = bpy.data.objects['RotationCamera']

# clear all previous animation data
myobj.animation_data_clear()

# set first and last frame index
bpy.context.scene.frame_start = 0
bpy.context.scene.frame_end = 5

# animate rotation in different angles
animate_rotation(x_angle_in_deg = 90,   frame_start = 0,   frame_end = 3)
animate_rotation(x_angle_in_deg = 0,    frame_start = 4,   frame_end = 4)
animate_rotation(x_angle_in_deg = 180,  frame_start = 5,   frame_end = 5)
#animate_rotation(x_angle_in_deg = 45,   frame_start = 10,  frame_end = 14)
#animate_rotation(x_angle_in_deg = 135,  frame_start = 15,  frame_end = 19)
