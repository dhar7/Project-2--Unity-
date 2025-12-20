#include "register_types.h"

#include <gdextension_interface.h>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/godot.hpp>

#include "SteeringWheel.h"

using namespace godot;

// --- Initialization & Registration ---

/**
 * @brief Registers the LogiSteeringWheel class with Godot's ClassDB.
 * * This function is called when the GDExtension is loaded.
 */
void initialize_logi_steering_wheel_module(ModuleInitializationLevel p_level) {
	// We only want to register our class once the core Godot systems are ready.
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}

	// Register the C++ class, mapping it to the "LogiSteeringWheel" name in GDScript.
	ClassDB::register_class<LogiSteeringWheel>();
}

/**
 * @brief Handles cleanup when the GDExtension is unloaded.
 * * This function is called when the GDExtension is unloaded.
 */
void uninitialize_logi_steering_wheel_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}

	// No custom unregistration logic is needed here for a simple class registration.
}

// --- Entry Point for Godot ---

/**
 * @brief The main entry point function Godot calls to load the GDExtension library.
 */
extern "C" {
// This defines the necessary entry-point symbol for the GDExtension
GDExtensionBool GDE_EXPORT logi_steering_wheel_library_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, const GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
	godot::GDExtensionBinding::InitObject init_object(p_get_proc_address, p_library, r_initialization);

	// Set the initialization and uninitialization functions for the module
	init_object.register_initializer(initialize_logi_steering_wheel_module);
	init_object.register_terminator(uninitialize_logi_steering_wheel_module);

	// We only register our class at the Scene level (MODULE_INITIALIZATION_LEVEL_SCENE)
	init_object.set_minimum_library_initialization_level(MODULE_INITIALIZATION_LEVEL_SCENE);

	return init_object.init();
}
}
