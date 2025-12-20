extends Node

var logi_wheel = null

const DEVICE_INDEX = 0

func _ready():
	if ClassDB.class_exists("LogiSteeringWheel"):
		logi_wheel = LogiSteeringWheel.new()
	else:
		print("ERROR: LogiSteeringWheel GDExtension is not available.")

func _process(delta):
	if logi_wheel and logi_wheel.is_connected(DEVICE_INDEX):
		var button_4_pressed = logi_wheel.button_is_pressed(DEVICE_INDEX, 4)
		var button_5_pressed = logi_wheel.button_is_pressed(DEVICE_INDEX, 5)
		if button_4_pressed or button_5_pressed:
			logi_wheel.play_constant_force(DEVICE_INDEX, 100)

func _exit_tree():
	if logi_wheel:
		logi_wheel.shutdown()
