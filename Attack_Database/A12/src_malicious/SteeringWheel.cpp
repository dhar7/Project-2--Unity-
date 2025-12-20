#include "SteeringWheel.h"

// Note: The LogitechSteeringWheelLib.h is included inside logi_steering_wheel.h.
// This ensures that all required types (HWND, DIJOYSTATE2ENGINES, etc.) and 
// function prototypes are available here.

using namespace godot;

// --- Internal Conversion Helpers ---

/**
 * @brief Internal function to map the complex DIJOYSTATE2ENGINES struct
 * (provided by the SDK) to a simplified, internally used state struct.
 * @param index Controller index (0-3).
 * @return LogiSimplifiedState
 */
LogiSimplifiedState LogiSteeringWheel::_get_simplified_state(int index) {
    LogiSimplifiedState simplified_state = {};
    
    // Call the actual SDK function to get the DirectInput state
    DIJOYSTATE2ENGINES *sdk_state = LogiGetStateENGINES(index);

    if (sdk_state) {
        // --- AXES ---
        // Map DInput axes (-10000 to 10000) to our simplified state
        
        simplified_state.wheel = sdk_state->lX;
        simplified_state.accelerator = sdk_state->lZ;    // Gas (typically Z-axis)
        simplified_state.brake = sdk_state->lRz;         // Brake (typically Rz-axis)
        simplified_state.clutch = sdk_state->rglSlider[0]; // Clutch (typically the first slider)

        // --- POV ---
        simplified_state.pov = sdk_state->rgdwPOV[0];

        // --- BUTTONS ---
        // Copy the 128 button states (0x80 for pressed, 0x00 for released)
        memcpy(simplified_state.buttons, sdk_state->rgbButtons, 128);
    }
    return simplified_state;
}

/**
 * @brief Converts the internal simplified state struct into a Godot Dictionary 
 * for easy access in GDScript.
 * @param state The simplified state.
 * @return Dictionary containing axis and button data.
 */
Dictionary LogiSteeringWheel::_state_to_dictionary(const LogiSimplifiedState& state) {
    Dictionary dict;
    
    // Axes
    dict["wheel"] = state.wheel;
    dict["accelerator"] = state.accelerator;
    dict["brake"] = state.brake;
    dict["clutch"] = state.clutch;

    // POV (0-35900 degrees * 100, 65535 for center/not pressed)
    dict["pov"] = state.pov; 

    // Buttons
    // Convert 128 button states (chars) into a PackedByteArray for efficiency in Godot
    PackedByteArray buttons_array;
    buttons_array.resize(128);
    for (int i = 0; i < 128; i++) {
        // Logitech uses 0x80 for pressed, 0x00 for released.
        // We convert this into a boolean-like integer (1 or 0)
        buttons_array[i] = (state.buttons[i] & 0x80) ? 1 : 0;
    }
    dict["buttons"] = buttons_array;

    return dict;
}

// --- Class Lifecycle ---

LogiSteeringWheel::LogiSteeringWheel() {
    // Initialization of the SDK is deferred until the user calls initialize()
}

LogiSteeringWheel::~LogiSteeringWheel() {
    // Ensure the SDK is shut down when the Godot object is destroyed
    LogiSteeringShutdown();
}

// --- GDExtension Method Binding ---

void LogiSteeringWheel::_bind_methods() {
    // --- Setup and Status ---
	ClassDB::bind_method(D_METHOD("initialize", "ignore_xinput_controllers"), &LogiSteeringWheel::initialize);
    ClassDB::bind_method(D_METHOD("shutdown"), &LogiSteeringWheel::shutdown);
    // Note: 'hwnd' is passed as uintptr_t from GDScript/Godot to hold the window handle
    ClassDB::bind_method(D_METHOD("initialize_with_window", "ignore_xinput_controllers", "hwnd"), &LogiSteeringWheel::initialize_with_window);
    ClassDB::bind_method(D_METHOD("get_sdk_version_dict"), &LogiSteeringWheel::get_sdk_version_dict);
    
    // --- Controller Management ---
    ClassDB::bind_method(D_METHOD("update"), &LogiSteeringWheel::update);
    ClassDB::bind_method(D_METHOD("get_state", "index"), &LogiSteeringWheel::get_state);
    ClassDB::bind_method(D_METHOD("get_device_path", "index"), &LogiSteeringWheel::get_device_path);
    ClassDB::bind_method(D_METHOD("get_friendly_product_name", "index"), &LogiSteeringWheel::get_friendly_product_name);
    ClassDB::bind_method(D_METHOD("is_connected", "index"), &LogiSteeringWheel::is_connected);
    ClassDB::bind_method(D_METHOD("is_device_connected_type", "index", "device_type"), &LogiSteeringWheel::is_device_connected_type);
    ClassDB::bind_method(D_METHOD("is_manufacturer_connected", "index", "manufacturer_type"), &LogiSteeringWheel::is_manufacturer_connected);
    ClassDB::bind_method(D_METHOD("is_model_connected", "index", "model_type"), &LogiSteeringWheel::is_model_connected);
    
    // --- Buttons ---
    ClassDB::bind_method(D_METHOD("button_triggered", "index", "button_nbr"), &LogiSteeringWheel::button_triggered);
    ClassDB::bind_method(D_METHOD("button_released", "index", "button_nbr"), &LogiSteeringWheel::button_released);
    ClassDB::bind_method(D_METHOD("button_is_pressed", "index", "button_nbr"), &LogiSteeringWheel::button_is_pressed);
    
    // --- Non-Linearity ---
    ClassDB::bind_method(D_METHOD("generate_non_linear_values", "index", "non_lin_coeff"), &LogiSteeringWheel::generate_non_linear_values);
    ClassDB::bind_method(D_METHOD("get_non_linear_value", "index", "input_value"), &LogiSteeringWheel::get_non_linear_value);

    // --- Force Feedback ---
    ClassDB::bind_method(D_METHOD("has_force_feedback", "index"), &LogiSteeringWheel::has_force_feedback);
    ClassDB::bind_method(D_METHOD("is_playing_force_type", "index", "force_type"), &LogiSteeringWheel::is_playing_force_type); 

    // Forces
    ClassDB::bind_method(D_METHOD("play_spring_force", "index", "offset_percentage", "saturation_percentage", "coefficient_percentage"), &LogiSteeringWheel::play_spring_force);
    ClassDB::bind_method(D_METHOD("stop_spring_force", "index"), &LogiSteeringWheel::stop_spring_force);
    ClassDB::bind_method(D_METHOD("play_constant_force", "index", "magnitude_percentage"), &LogiSteeringWheel::play_constant_force);
    ClassDB::bind_method(D_METHOD("stop_constant_force", "index"), &LogiSteeringWheel::stop_constant_force);
    ClassDB::bind_method(D_METHOD("play_damper_force", "index", "coefficient_percentage"), &LogiSteeringWheel::play_damper_force);
    ClassDB::bind_method(D_METHOD("stop_damper_force", "index"), &LogiSteeringWheel::stop_damper_force);
    ClassDB::bind_method(D_METHOD("play_side_collision_force", "index", "magnitude_percentage"), &LogiSteeringWheel::play_side_collision_force);
    ClassDB::bind_method(D_METHOD("play_frontal_collision_force", "index", "magnitude_percentage"), &LogiSteeringWheel::play_frontal_collision_force);
    ClassDB::bind_method(D_METHOD("play_dirt_road_effect", "index", "magnitude_percentage"), &LogiSteeringWheel::play_dirt_road_effect);
    ClassDB::bind_method(D_METHOD("stop_dirt_road_effect", "index"), &LogiSteeringWheel::stop_dirt_road_effect);
    ClassDB::bind_method(D_METHOD("play_bumpy_road_effect", "index", "magnitude_percentage"), &LogiSteeringWheel::play_bumpy_road_effect);
    ClassDB::bind_method(D_METHOD("stop_bumpy_road_effect", "index"), &LogiSteeringWheel::stop_bumpy_road_effect);
    ClassDB::bind_method(D_METHOD("play_slippery_road_effect", "index", "magnitude_percentage"), &LogiSteeringWheel::play_slippery_road_effect);
    ClassDB::bind_method(D_METHOD("stop_slippery_road_effect", "index"), &LogiSteeringWheel::stop_slippery_road_effect);
    ClassDB::bind_method(D_METHOD("play_surface_effect", "index", "periodic_type", "magnitude_percentage", "period"), &LogiSteeringWheel::play_surface_effect);
    ClassDB::bind_method(D_METHOD("stop_surface_effect", "index"), &LogiSteeringWheel::stop_surface_effect);
    ClassDB::bind_method(D_METHOD("play_car_airborne", "index"), &LogiSteeringWheel::play_car_airborne);
    ClassDB::bind_method(D_METHOD("stop_car_airborne", "index"), &LogiSteeringWheel::stop_car_airborne);
    ClassDB::bind_method(D_METHOD("play_softstop_force", "index", "usable_range_percentage"), &LogiSteeringWheel::play_softstop_force);
    ClassDB::bind_method(D_METHOD("stop_softstop_force", "index"), &LogiSteeringWheel::stop_softstop_force);

    // --- Properties and Settings ---
    ClassDB::bind_method(D_METHOD("set_preferred_controller_properties", "properties_dict"), &LogiSteeringWheel::set_preferred_controller_properties);
    ClassDB::bind_method(D_METHOD("get_current_controller_properties", "index"), &LogiSteeringWheel::get_current_controller_properties);
    ClassDB::bind_method(D_METHOD("get_shifter_mode", "index"), &LogiSteeringWheel::get_shifter_mode);
    ClassDB::bind_method(D_METHOD("set_operating_range", "index", "range"), &LogiSteeringWheel::set_operating_range);
    ClassDB::bind_method(D_METHOD("get_operating_range", "index"), &LogiSteeringWheel::get_operating_range);

    // --- LEDs ---
    ClassDB::bind_method(D_METHOD("play_leds", "index", "current_rpm", "rpm_first_led_turns_on", "rpm_red_line"), &LogiSteeringWheel::play_leds);

    // --- Expose SDK Constants to GDScript ---
    // These constants are pulled from LogitechSteeringWheelLib.h via the header file
    
    // Force Types
    BIND_CONSTANT(LOGI_FORCE_NONE);
    BIND_CONSTANT(LOGI_FORCE_SPRING);
    BIND_CONSTANT(LOGI_FORCE_CONSTANT);
    BIND_CONSTANT(LOGI_FORCE_DAMPER);
    BIND_CONSTANT(LOGI_FORCE_SIDE_COLLISION);
    BIND_CONSTANT(LOGI_FORCE_FRONTAL_COLLISION);
    BIND_CONSTANT(LOGI_FORCE_DIRT_ROAD);
    BIND_CONSTANT(LOGI_FORCE_BUMPY_ROAD);
    BIND_CONSTANT(LOGI_FORCE_SLIPPERY_ROAD);
    BIND_CONSTANT(LOGI_FORCE_SURFACE_EFFECT);
    BIND_CONSTANT(LOGI_FORCE_SOFTSTOP);
    BIND_CONSTANT(LOGI_FORCE_CAR_AIRBORNE);

    // Periodic Types
    BIND_CONSTANT(LOGI_PERIODICTYPE_NONE);
    BIND_CONSTANT(LOGI_PERIODICTYPE_SINE);
    BIND_CONSTANT(LOGI_PERIODICTYPE_SQUARE);
    BIND_CONSTANT(LOGI_PERIODICTYPE_TRIANGLE);

    // Device Types
    BIND_CONSTANT(LOGI_DEVICE_TYPE_NONE);
    BIND_CONSTANT(LOGI_DEVICE_TYPE_WHEEL);
    BIND_CONSTANT(LOGI_DEVICE_TYPE_JOYSTICK);
    BIND_CONSTANT(LOGI_DEVICE_TYPE_GAMEPAD);
    BIND_CONSTANT(LOGI_DEVICE_TYPE_OTHER);

    // Manufacturer Types
    BIND_CONSTANT(LOGI_MANUFACTURER_NONE);
    BIND_CONSTANT(LOGI_MANUFACTURER_LOGITECH);
    BIND_CONSTANT(LOGI_MANUFACTURER_MICROSOFT);
    BIND_CONSTANT(LOGI_MANUFACTURER_OTHER);

    // Model Types (Binding only a few key models for example)
    BIND_CONSTANT(LOGI_MODEL_G27);
    BIND_CONSTANT(LOGI_MODEL_DRIVING_FORCE_GT);
    BIND_CONSTANT(LOGI_MODEL_G25);
    BIND_CONSTANT(LOGI_MODEL_MOMO_RACING);
    BIND_CONSTANT(LOGI_MODEL_G29);
    BIND_CONSTANT(LOGI_MODEL_G920);
}

// --- Public Method Implementations (Wrapping SDK Functions) ---

bool LogiSteeringWheel::initialize(bool ignore_xinput_controllers) {
    return LogiSteeringInitialize(ignore_xinput_controllers);
}

void LogiSteeringWheel::shutdown() {
    LogiSteeringShutdown();
}

bool LogiSteeringWheel::initialize_with_window(bool ignore_xinput_controllers, uintptr_t hwnd) {
    // Cast the uintptr_t (which holds the window handle from Godot) back to HWND
    return LogiSteeringInitializeWithWindow(ignore_xinput_controllers, (HWND)hwnd);
}

Dictionary LogiSteeringWheel::get_sdk_version_dict() {
    int major = 0, minor = 0, build = 0;
    Dictionary version_dict;
    if (LogiSteeringGetSdkVersion(&major, &minor, &build)) {
        version_dict["major"] = major;
        version_dict["minor"] = minor;
        version_dict["build"] = build;
    } else {
        version_dict["error"] = "Failed to get SDK version.";
    }
    return version_dict;
}

bool LogiSteeringWheel::update() {
    return LogiUpdate();
}

Dictionary LogiSteeringWheel::get_state(int index) {
    LogiSimplifiedState state = _get_simplified_state(index);
    return _state_to_dictionary(state);
}

String LogiSteeringWheel::get_device_path(int index) {
    wchar_t buffer[256]; 
    if (LogiGetDevicePath(index, buffer, 256)) {
        // Convert wide char (wchar_t) string to Godot String
        return String(buffer);
    }
    return "";
}

String LogiSteeringWheel::get_friendly_product_name(int index) {
    wchar_t buffer[256];
    if (LogiGetFriendlyProductName(index, buffer, 256)) {
        // Convert wide char (wchar_t) string to Godot String
        return String(buffer);
    }
    return "";
}

bool LogiSteeringWheel::is_connected(int index) {
    return LogiIsConnected(index);
}

bool LogiSteeringWheel::is_device_connected_type(int index, int device_type) {
    return LogiIsDeviceConnected(index, device_type);
}

bool LogiSteeringWheel::is_manufacturer_connected(int index, int manufacturer_type) {
    return LogiIsManufacturerConnected(index, manufacturer_type);
}

bool LogiSteeringWheel::is_model_connected(int index, int model_type) {
    return LogiIsModelConnected(index, model_type);
}

bool LogiSteeringWheel::button_triggered(int index, int button_nbr) {
    return LogiButtonTriggered(index, button_nbr);
}

bool LogiSteeringWheel::button_released(int index, int button_nbr) {
    return LogiButtonReleased(index, button_nbr);
}

bool LogiSteeringWheel::button_is_pressed(int index, int button_nbr) {
    return LogiButtonIsPressed(index, button_nbr);
}

bool LogiSteeringWheel::generate_non_linear_values(int index, int non_lin_coeff) {
    return LogiGenerateNonLinearValues(index, non_lin_coeff);
}

int LogiSteeringWheel::get_non_linear_value(int index, int input_value) {
    return LogiGetNonLinearValue(index, input_value);
}

bool LogiSteeringWheel::has_force_feedback(int index) {
    return LogiHasForceFeedback(index);
}

bool LogiSteeringWheel::is_playing_force_type(int index, int force_type) {
    return LogiIsPlaying(index, force_type);
}

bool LogiSteeringWheel::play_spring_force(int index, int offset_percentage, int saturation_percentage, int coefficient_percentage) {
    return LogiPlaySpringForce(index, offset_percentage, saturation_percentage, coefficient_percentage);
}

bool LogiSteeringWheel::stop_spring_force(int index) {
    return LogiStopSpringForce(index);
}

bool LogiSteeringWheel::play_constant_force(int index, int magnitude_percentage) {
    return LogiPlayConstantForce(index, magnitude_percentage);
}

bool LogiSteeringWheel::stop_constant_force(int index) {
    return LogiStopConstantForce(index);
}

bool LogiSteeringWheel::play_damper_force(int index, int coefficient_percentage) {
    return LogiPlayDamperForce(index, coefficient_percentage);
}

bool LogiSteeringWheel::stop_damper_force(int index) {
    return LogiStopDamperForce(index);
}

bool LogiSteeringWheel::play_side_collision_force(int index, int magnitude_percentage) {
    return LogiPlaySideCollisionForce(index, magnitude_percentage);
}

bool LogiSteeringWheel::play_frontal_collision_force(int index, int magnitude_percentage) {
    return LogiPlayFrontalCollisionForce(index, magnitude_percentage);
}

bool LogiSteeringWheel::play_dirt_road_effect(int index, int magnitude_percentage) {
    return LogiPlayDirtRoadEffect(index, magnitude_percentage);
}

bool LogiSteeringWheel::stop_dirt_road_effect(int index) {
    return LogiStopDirtRoadEffect(index);
}

bool LogiSteeringWheel::play_bumpy_road_effect(int index, int magnitude_percentage) {
    return LogiPlayBumpyRoadEffect(index, magnitude_percentage);
}

bool LogiSteeringWheel::stop_bumpy_road_effect(int index) {
    return LogiStopBumpyRoadEffect(index);
}

bool LogiSteeringWheel::play_slippery_road_effect(int index, int magnitude_percentage) {
    return LogiPlaySlipperyRoadEffect(index, magnitude_percentage);
}

bool LogiSteeringWheel::stop_slippery_road_effect(int index) {
    return LogiStopSlipperyRoadEffect(index);
}

bool LogiSteeringWheel::play_surface_effect(int index, int periodic_type, int magnitude_percentage, int period) {
    return LogiPlaySurfaceEffect(index, periodic_type, magnitude_percentage, period);
}

bool LogiSteeringWheel::stop_surface_effect(int index) {
    return LogiStopSurfaceEffect(index);
}

bool LogiSteeringWheel::play_car_airborne(int index) {
    return LogiPlayCarAirborne(index);
}

bool LogiSteeringWheel::stop_car_airborne(int index) {
    return LogiStopCarAirborne(index);
}

bool LogiSteeringWheel::play_softstop_force(int index, int usable_range_percentage) {
    return LogiPlaySoftstopForce(index, usable_range_percentage);
}

bool LogiSteeringWheel::stop_softstop_force(int index) {
    return LogiStopSoftstopForce(index);
}

bool LogiSteeringWheel::set_preferred_controller_properties(Dictionary properties_dict) {
    LogiControllerPropertiesData properties;

    // Map Godot Dictionary to the SDK's LogiControllerPropertiesData struct
    // Use default values if keys are missing
    properties.forceEnable = properties_dict.has("force_enable") ? (bool)properties_dict["force_enable"] : true;
    properties.overallGain = properties_dict.has("overall_gain") ? (int)properties_dict["overall_gain"] : 100;
    properties.springGain = properties_dict.has("spring_gain") ? (int)properties_dict["spring_gain"] : 100;
    properties.damperGain = properties_dict.has("damper_gain") ? (int)properties_dict["damper_gain"] : 100;
    properties.defaultSpringEnabled = properties_dict.has("default_spring_enabled") ? (bool)properties_dict["default_spring_enabled"] : true;
    properties.defaultSpringGain = properties_dict.has("default_spring_gain") ? (int)properties_dict["default_spring_gain"] : 100;
    properties.combinePedals = properties_dict.has("combine_pedals") ? (bool)properties_dict["combine_pedals"] : false;
    properties.wheelRange = properties_dict.has("wheel_range") ? (int)properties_dict["wheel_range"] : 900;
    properties.gameSettingsEnabled = properties_dict.has("game_settings_enabled") ? (bool)properties_dict["game_settings_enabled"] : true;
    properties.allowGameSettings = properties_dict.has("allow_game_settings") ? (bool)properties_dict["allow_game_settings"] : true;

    return LogiSetPreferredControllerProperties(properties);
}

Dictionary LogiSteeringWheel::get_current_controller_properties(int index) {
    LogiControllerPropertiesData properties;
    Dictionary dict;
    
    // LogiGetCurrentControllerProperties fills the 'properties' struct
    if (LogiGetCurrentControllerProperties(index, properties)) {
        // Map the struct fields back to a Godot Dictionary
        dict["force_enable"] = properties.forceEnable;
        dict["overall_gain"] = properties.overallGain;
        dict["spring_gain"] = properties.springGain;
        dict["damper_gain"] = properties.damperGain;
        dict["default_spring_enabled"] = properties.defaultSpringEnabled;
        dict["default_spring_gain"] = properties.defaultSpringGain;
        dict["combine_pedals"] = properties.combinePedals;
        dict["wheel_range"] = properties.wheelRange;
        dict["game_settings_enabled"] = properties.gameSettingsEnabled;
        dict["allow_game_settings"] = properties.allowGameSettings;
    } else {
        dict["error"] = "Failed to get controller properties. Device may not be connected or initialized.";
    }

    return dict;
}

int LogiSteeringWheel::get_shifter_mode(int index) {
    return LogiGetShifterMode(index);
}

bool LogiSteeringWheel::set_operating_range(int index, int range) {
    return LogiSetOperatingRange(index, range);
}

int LogiSteeringWheel::get_operating_range(int index) {
    int range = 0;
    // LogiGetOperatingRange takes a reference, so it fills the 'range' integer
    LogiGetOperatingRange(index, range);
    return range;
}

bool LogiSteeringWheel::play_leds(int index, float current_rpm, float rpm_first_led_turns_on, float rpm_red_line) {
    return LogiPlayLeds(index, current_rpm, rpm_first_led_turns_on, rpm_red_line);
}
