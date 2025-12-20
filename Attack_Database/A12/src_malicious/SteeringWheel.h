#ifndef LOGI_STEERING_WHEEL_H
#define LOGI_STEERING_WHEEL_H

#include <godot_cpp/classes/ref_counted.hpp>
#include <godot_cpp/variant/utility_functions.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/dictionary.hpp>
#include <godot_cpp/variant/string.hpp>
#include <godot_cpp/variant/typed_array.hpp>

// --- SDK Dependency ---
// Directly include the Logitech SDK header. 
// The SDK's structs (LogiControllerPropertiesData, DIJOYSTATE2ENGINES) and 
// constants (LOGI_FORCE_SPRING, LOGI_MODEL_G29, etc.) are now defined here.

#include "LogitechSteeringWheelLib.h"

// Internal Simplified State for Godot return values
// NOTE: We keep this because we want to simplify the complex DIJOYSTATE2ENGINES 
// struct into a clean, easy-to-use format before converting it to a Godot Dictionary.
typedef struct LogiSimplifiedState
{
    // Axes are typically -10000 to 10000
    int wheel;
    int accelerator;
    int brake;
    int clutch;

    // POV values: 0 to 35900 (degrees * 100), 65535 for center/not pressed
    unsigned int pov;

    // Buttons: 0 to 127. 1 means pressed, 0 means not pressed
    unsigned char buttons[128];

} LogiSimplifiedState;


// --- GDExtension Class Definition ---

namespace godot {

class LogiSteeringWheel : public RefCounted {
	GDCLASS(LogiSteeringWheel, RefCounted)

private:
    // Internal function to map the DIJOYSTATE2ENGINES structure to our simplified state
    // DIJOYSTATE2ENGINES is now defined via the SDK header include.
    LogiSimplifiedState _get_simplified_state(int index);
    
    // Internal function to convert our simplified state to a Godot Dictionary
    Dictionary _state_to_dictionary(const LogiSimplifiedState& state);

protected:
	static void _bind_methods();

public:
	LogiSteeringWheel();
	~LogiSteeringWheel();

    // --- Setup and Status ---
	bool initialize(bool ignore_xinput_controllers);
    void shutdown();
    // HWND will be passed as uintptr_t from Godot. HWND is defined in the SDK header's includes.
    bool initialize_with_window(bool ignore_xinput_controllers, uintptr_t hwnd); 
    
    // Versioning
    Dictionary get_sdk_version_dict();
    
    // --- Controller Management ---
    bool update();
    Dictionary get_state(int index);
    
    String get_device_path(int index);
    String get_friendly_product_name(int index);
    bool is_connected(int index);
    bool is_device_connected_type(int index, int device_type); 
    bool is_manufacturer_connected(int index, int manufacturer_type); 
    bool is_model_connected(int index, int model_type); 
    
    // --- Buttons ---
    bool button_triggered(int index, int button_nbr);
    bool button_released(int index, int button_nbr);
    bool button_is_pressed(int index, int button_nbr);
    
    // --- Non-Linearity ---
    bool generate_non_linear_values(int index, int non_lin_coeff);
    int get_non_linear_value(int index, int input_value);

    // --- Force Feedback ---
    bool has_force_feedback(int index);
    bool is_playing_force_type(int index, int force_type); 

    // Forces
    bool play_spring_force(int index, int offset_percentage, int saturation_percentage, int coefficient_percentage);
    bool stop_spring_force(int index);
    bool play_constant_force(int index, int magnitude_percentage);
    bool stop_constant_force(int index);
    bool play_damper_force(int index, int coefficient_percentage);
    bool stop_damper_force(int index);
    bool play_side_collision_force(int index, int magnitude_percentage);
    bool play_frontal_collision_force(int index, int magnitude_percentage);
    bool play_dirt_road_effect(int index, int magnitude_percentage);
    bool stop_dirt_road_effect(int index);
    bool play_bumpy_road_effect(int index, int magnitude_percentage);
    bool stop_bumpy_road_effect(int index);
    bool play_slippery_road_effect(int index, int magnitude_percentage);
    bool stop_slippery_road_effect(int index);
    bool play_surface_effect(int index, int periodic_type, int magnitude_percentage, int period); 
    bool stop_surface_effect(int index);
    bool play_car_airborne(int index);
    bool stop_car_airborne(int index);
    bool play_softstop_force(int index, int usable_range_percentage);
    bool stop_softstop_force(int index);

    // --- Properties and Settings ---
    bool set_preferred_controller_properties(Dictionary properties_dict);
    Dictionary get_current_controller_properties(int index);
    int get_shifter_mode(int index);
    bool set_operating_range(int index, int range);
    int get_operating_range(int index);

    // --- LEDs ---
    bool play_leds(int index, float current_rpm, float rpm_first_led_turns_on, float rpm_red_line);

    // Functions that take DInput devices are not exposed directly to GDScript
    // since Godot does not expose the LPDIRECTINPUTDEVICE8 type.
};

} // namespace godot

#endif // LOGI_STEERING_WHEEL_H
