#ifndef REGISTER_TYPES_H
#define REGISTER_TYPES_H

#include <gdextension_interface.h>
#include <godot_cpp/core/defs.hpp>
#include <godot_cpp/godot.hpp>

using namespace godot;

/**
 * @brief Called when the GDExtension module is initialized.
 * * @param p_level The current initialization level. We only register our class at 
 * the MODULE_INITIALIZATION_LEVEL_SCENE level.
 */
void initialize_logi_steering_wheel_module(ModuleInitializationLevel p_level);

/**
 * @brief Called when the GDExtension module is terminated.
 * * @param p_level The current initialization level.
 */
void uninitialize_logi_steering_wheel_module(ModuleInitializationLevel p_level);

#endif // REGISTER_TYPES_H
