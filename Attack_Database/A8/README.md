### Implementation Steps

1. **Source Integration:**
* Move the `AssetImportNotifier.h` and `AssetImportNotifier.cpp` files into the `Source\SteeringWheel` directory.
* This handles the initial package import logic.


2. **Initial Build:**
* Right-click the `.uproject` file and select **Generate Visual Studio project files**.
* Open the generated `.sln` file in **Visual Studio** and build the solution.


3. **Code Generation Trigger:**
* Open the project in the **Unreal Editor**.
* Upon opening, notice that two new C++ files are automatically generated in the `Source\SteeringWheelInterface` directory.


4. **Final Compilation:**
* **Repeat the build process:** Close the editor, regenerate the Visual Studio solution, and build in VS once more to compile the newly generated code.
* Re-open the project in Unreal.


5. **Execution:**
* Press **Play** in the editor to execute the integrated logic.