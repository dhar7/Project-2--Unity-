## 1. Attack via `_ready()` (GDScript)

This method leverages the initialization function in GDScript to execute logic as soon as a node enters the scene tree.

### Steps to Reproduce:

1. **Open Project:** Open the Godot project located in the `steering-wheel-godot` directory.
2. **Import Package:** Import the ZIP file found in `malicious_package[Godot]`.
3. **Setup Node:** * Create a new Node in the scene.
* Attach the `initial.gd` script to this node.


4. **Execution:** Press **Play**.
* *Note: A Logitech steering wheel must be connected for the script to trigger as intended.*



---

## 2. ATF (Advanced Threat Factor) in GDExtension

This method involves modifying the C++ source code of a GDExtension to execute malicious logic at the library initialization level.

### Build Instructions:

1. **Modify Source:** * Navigate to `src/register_types.cpp`.
* Locate the `logi_steering_wheel_library_init` function.
* Insert your logic/payload within this function.


2. **Compile:** * Open your command line in the root directory (where the `SConstruct` file is located).
* Run the following command:
```bash
scons platform=windows

```


* *If you do not have SCons installed, refer to the [official Godot GDExtension tutorial](https://docs.godotengine.org/en/stable/tutorials/scripting/gdextension/gdextension_c_example.html).*



### Deployment:

* The compiled library will be generated in `steering-wheel-godot/bin`.
* The logic executes automatically when the project is played.
* **Distribution:** To deploy to a target project, copy the following files to the target's `bin/` folder:
* `logi_steering_wheel_gde.lib`
* `LogiSteeringWheel.gdextension`