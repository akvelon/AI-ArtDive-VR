import glob
import os
import subprocess
import shutil
import argparse
import time
import re
from colorama import Fore
from colorama import Back
from colorama import Style

def execute_blender(working_dir_path, 
                    blender_app_file_path,
                    blender_python_script_path, 
                    panorama_file_path, 
                    render_output_dir_path):
    # working directory absolute path
    working_dir_abs_path = os.path.abspath(working_dir_path)

    # blender app absolute path
    blender_app_file_abs_path = os.path.abspath(blender_app_file_path)

    # blender internal python script
    blender_python_script_abs_path = os.path.abspath(blender_python_script_path)

    # source panorama absolute path
    panorama_file_abs_path = os.path.abspath(panorama_file_path)

    # render output directory & target files mask
    render_output_dir_abs_path = os.path.abspath(render_output_dir_path)
    render_output_files_abs_path_mask = os.path.join(render_output_dir_abs_path, "####")

    # prepare blender command line
    blender_command_line = [blender_app_file_abs_path,
                            "-noaudio",
                            "--background",
                            panorama_file_abs_path,
                            "--python-exit-code",
                            "10",
                            "--python",
                            blender_python_script_abs_path,
                            "--render-output",
                            render_output_files_abs_path_mask, 
                            "--render-anim"]

    # run blender with 1h timeout
    subprocess.run(blender_command_line, cwd=working_dir_abs_path, check=True, timeout=3600)

def execute_deep_art_app_repeated(working_dir_path,
                                  deep_art_app_file_path,
                                  input_images_dir_path,
                                  output_images_dir_path,
                                  effect_name,
                                  tries_count):
    for i in range(tries_count):
        try:
            execute_deep_art_app(working_dir_path,
                                 deep_art_app_file_path,
                                 input_images_dir_path,
                                 output_images_dir_path,
                                 effect_name)
            return
        except:
            print(f"[TRY {i}] execute_deep_art_app() failed: {excepion}")
            
            if (i < 3):
                time.sleep(30) # wait 30 secs
            else:
                time.sleep(90) # wait 90 secs

    raise Exception(f"Dexecute_deep_art_app() failed, tries count {tries_count} exceeded!")

def execute_deep_art_app(working_dir_path,
                         deep_art_app_file_path,
                         input_images_dir_path,
                         output_images_dir_path,
                         effect_name):
    # check on no-effect to apply
    if effect_name == "":
        return

    # working directory absolute path
    working_dir_abs_path = os.path.abspath(working_dir_path)

    # deep-art app absolute path
    deep_art_app_file_abs_path = os.path.abspath(deep_art_app_file_path)

    # input images dir absolute path
    input_images_dir_abs_path = os.path.abspath(input_images_dir_path)

    # output images dir absolute path
    output_images_dir_abs_path = os.path.abspath(output_images_dir_path)

    # prepare deep-art app command line
    deep_art_app_command_line = [deep_art_app_file_abs_path,
                                 "--silent-mode",
                                 "--inputDirectory",
                                 input_images_dir_abs_path,
                                 "--outputDirectory",
                                 output_images_dir_abs_path,
                                 "--save-reports",
                                 "All",
                                 "--parallelism-degree",
                                 "10",
                                 "--effect",
                                 effect_name]

    # run deep-art app with 1h timeout
    subprocess.run(deep_art_app_command_line, cwd=working_dir_abs_path, check=True, timeout=3600)

def execute_nona(working_dir_path,
                 nona_file_path,
                 project_file_path):
    # working directory absolute path
    working_dir_abs_path = os.path.abspath(working_dir_path)

    # prepare nona util command line
    nona_command_line = [nona_file_path,
                         "-v",
                         "-z",
                         "LZW",
                         "-r",
                         "ldr",
                         "-m",
                         "TIFF_m",
                         "-o",
                         "panorama",
                         project_file_path]

    # run nona util with 1h timeout (make transformed images)
    subprocess.run(nona_command_line, cwd=working_dir_abs_path, check=True, timeout=3600)

def execute_enblend(working_dir_path,
                    enblend_file_path):
    # working directory absolute path
    working_dir_abs_path = os.path.abspath(working_dir_path)

    # to prepare masks with win32 version of the enblend
    # "/mnt/c/Program Files/Hugin/bin/enblend.exe" --save-masks --fine-mask --no-optimize --pre-assemble \
    #  -w -f10000x5000  --compression=90  -o "panorama.jpg" --  "panorama0000.tif" "panorama0001.tif" "panorama0002.tif" "panorama0003.tif" "panorama0004.tif" "panorama0005.tif" "panorama0006.tif" "panorama0007.tif" "panorama0008.tif" "panorama0009.tif" "panorama0010.tif" "panorama0011.tif" "panorama0012.tif" "panorama0013.tif" "panorama0014.tif" "panorama0015.tif" "panorama0016.tif" "panorama0017.tif" "panorama0018.tif" "panorama0019.tif" "panorama0020.tif" "panorama0021.tif" "panorama0022.tif" "panorama0023.tif" "panorama0024.tif" "panorama0025.tif" "panorama0026.tif" "panorama0027.tif" "panorama0028.tif" "panorama0029.tif" "panorama0030.tif" "panorama0031.tif" "panorama0032.tif" "panorama0033.tif" "panorama0034.tif" "panorama0035.tif" "panorama0036.tif" "panorama0037.tif" "panorama0038.tif" "panorama0039.tif" "panorama0040.tif" "panorama0041.tif" "panorama0042.tif" "panorama0043.tif" "panorama0044.tif" "panorama0045.tif" "panorama0046.tif" "panorama0047.tif" "panorama0048.tif" "panorama0049.tif" "panorama0050.tif"

    # prepare enblend util command line
    enblend_command_line = [enblend_file_path,
                            "--load-masks",
                            "--pre-assemble",
                            "-w",
                            "-f10000x5000",
                            "--compression=90",
                            "-o"
                            "panorama.jpg",
                            "--"]
    # add files to blend
    for i in range(0, 6):
        number = str(i).zfill(4)
        image_filename = f"panorama{number}.tif"
        enblend_command_line.append(image_filename)

    # run enblend util with 1h timeout (blend transformed by nona images)
    print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}{enblend_command_line}{Style.RESET_ALL}")

    subprocess.run(enblend_command_line, cwd=working_dir_abs_path, check=True, timeout=3600)

def process_panorama(working_dir_path,
                     mode,
                     effect):
    # working directory absolute path
    working_dir_abs_path = os.path.abspath(working_dir_path)
    images_dir_abs_path = os.path.join(working_dir_abs_path, "images/")

    if mode == "split" or mode == "all" or mode == "all-without-convert":
        # colored console output
        print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}Blender processing...{Style.RESET_ALL}")

        # prepare for blender
        shutil.copy("/app/data/blender_panorama.blend", working_dir_abs_path)

        # blender project file path
        blender_project_file_abs_path = os.path.join(working_dir_abs_path, "blender_panorama.blend")

        # run blender
        execute_blender(working_dir_path            = working_dir_abs_path,
                        blender_app_file_path       = "/usr/local/bin/blender",
                        blender_python_script_path  = "/app/src/blender/camera_animation.py",
                        panorama_file_path          = blender_project_file_abs_path,
                        render_output_dir_path      = images_dir_abs_path)

        # colored console output
        print(f"{Back.BLACK}{Fore.GREEN}Blender processing successfully FINISHED{Style.RESET_ALL}\r\n")

    if mode == "convert" or mode == "all":
        # colored console output
        print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}DeepArt processing...{Style.RESET_ALL}")

        # run deep-art converter
        execute_deep_art_app_repeated(working_dir_path       = images_dir_abs_path,
                                      deep_art_app_file_path = "/usr/local/bin/DeepArtConsoleClient",
                                      input_images_dir_path  = images_dir_abs_path,
                                      output_images_dir_path = images_dir_abs_path,
                                      effect_name            = effect,
                                      tries_count            = 5)
        
        # colored console output
        print(f"{Back.BLACK}{Fore.GREEN}DeepArt processing successfully FINISHED{Style.RESET_ALL}\r\n")

    if mode == "merge" or mode == "all" or mode == "all-without-convert":
        # colored console output
        print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}Hugin utils nona&enblend processing...{Style.RESET_ALL}")

        # copy project file
        shutil.copy("/app/data/hugin_panorama.pto", working_dir_abs_path)

        # hugin project file (for nona util)
        hugin_project_file_abs_path = os.path.join(working_dir_abs_path, "hugin_panorama.pto")

        # run nona
        print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}Starting nona...{Style.RESET_ALL}")

        execute_nona(working_dir_path  = images_dir_abs_path,
                     nona_file_path    = "/usr/local/bin/nona",
                     project_file_path = hugin_project_file_abs_path)

        # copy enblend masks
        for filename in glob.glob(os.path.join("/app/data/enblend_ref-masks", '*.tif')):
            shutil.copy(filename, images_dir_abs_path)

        # run enblend
        print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}Starting enblend...{Style.RESET_ALL}")

        execute_enblend(working_dir_path  = images_dir_abs_path,
                        enblend_file_path = "/usr/local/bin/enblend")

        # copy processed panorama to parent directory
        processed_panorama_src_abs_file_path = os.path.join(images_dir_abs_path, "panorama.jpg")

        # move panorama with different name
        if effect == "":
            processed_panorama_dst_abs_file_path = os.path.join(working_dir_abs_path, "panorama_##orig##.jpg")
        else:
            processed_panorama_dst_abs_file_path = os.path.join(working_dir_abs_path, f"panorama_{effect}.jpg")

        shutil.copyfile(processed_panorama_src_abs_file_path, processed_panorama_dst_abs_file_path)

        # colored console output
        print(f"{Back.BLACK}{Fore.GREEN}Hugin utils nona&enblend processing successfully FINISHED{Style.RESET_ALL}\r\n")

    #------------ cleaning up ----------------

    # colored console output
    print(f"{Back.BLACK}{Fore.LIGHTBLUE_EX}Cleaning up...{Style.RESET_ALL}")

    if mode == "split" or mode == "all" or mode == "all-without-convert":
        blender_panorama_project_file_abs_path = os.path.join(working_dir_abs_path, "blender_panorama.blend")
        os.remove(blender_panorama_project_file_abs_path)

    if mode == "convert" or mode == "merge" or mode == "all":
        for filename in os.listdir(images_dir_abs_path):
            file_path = os.path.join(images_dir_abs_path, filename)
            os.remove(file_path)
        os.rmdir(images_dir_abs_path)
    else:
        for filename in os.listdir(images_dir_abs_path):
            if not re.match("^[0-9]{4}.png$", filename):
                file_path = os.path.join(images_dir_abs_path, filename)
                os.remove(file_path)

    if mode == "merge" or mode == "all" or mode == "all-without-convert":
        hugin_panorama_project_file_abs_path = os.path.join(working_dir_abs_path, "hugin_panorama.pto")
        os.remove(hugin_panorama_project_file_abs_path)

    # colored console output
    print(f"{Back.BLACK}{Fore.GREEN}Cleaning up successfully FINISHED{Style.RESET_ALL}\r\n")

#-------------------------------------------------------------

try:
    # initialize DOTNET_BUNDLE_EXTRACT_BASE_DIR
    os.environ["DOTNET_BUNDLE_EXTRACT_BASE_DIR"] = "/tmp"

    # Instantiate the parser
    parser = argparse.ArgumentParser(description='Pano converter script')

    # arguments
    parser.add_argument('-m', '--mode',
                        action="store", dest="mode", default="all-without-convert",
                        help="Mode: split, convert, merge, all, all-without-convert")

    parser.add_argument('-e','--effects', 
                        nargs='+', dest="effects", default=["Comics", "Candy", "Twilight"], 
                        help='Effects: Comics, Candy, Twilight, Udnie, The Scream, VIII, Starry Night, Rain Princess, Watercolor, Pixel Art, Pen')

    # parse args
    args = parser.parse_args()

    # check mode value
    if args.mode not in ["split", "convert", "merge", "all", "all-without-convert"]:
        parser.error("mode: incorrect value!")

    # *.exr files mask
    panorama_files_mask = os.path.join("/gen", '*.exr')

    # process every EXR panorama file in /gen
    for filename in glob.glob(panorama_files_mask):
        # get filename without extension
        target_directory_name = os.path.splitext(filename)[0]

        # create sub-dirs
        os.makedirs(target_directory_name, exist_ok=True)

        # copy panorama to target directory as 'panorama_to_process.exr'
        target_panorama_file_path = os.path.join(target_directory_name, "panorama_to_process.exr")
        shutil.copyfile(filename, target_panorama_file_path)
        
        # ignore effects in "all-without-convert" mode
        if (args.mode != "all-without-convert"):
            # iterate over all effects provided
            for effect in args.effects:
                process_panorama(target_directory_name, args.mode, effect)

        # and original one (no-effect)
        process_panorama(target_directory_name, args.mode, "")
        
        # remove target panorama EXR-file
        os.remove(target_panorama_file_path)

except Exception as excepion:
    # colored console output
    print(f"{Back.WHITE}{Fore.RED}Error occured: {excepion}{Style.RESET_ALL}")