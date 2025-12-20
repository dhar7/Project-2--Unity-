extends Node2D

# Target the first connected controller
const DEVICE_INDEX = 0
var logi_wheel: LogiSteeringWheel = null

func _ready():
	if not LogiSteeringWheel:
		push_error("LogiSteeringWheel extension not found.")
		return
	
	logi_wheel = LogiSteeringWheel.new()
	
	# Initialize SDK (false means do not ignore XInput devices)
	if logi_wheel.initialize(false):
		print("SDK initialized.")
		# Stop initial forces
		logi_wheel.stop_spring_force(DEVICE_INDEX)
		logi_wheel.stop_constant_force(DEVICE_INDEX)
	else:
		push_error("SDK failed to initialize.")
		logi_wheel = null

func _process(delta):
	logi_wheel.update()
	logi_wheel.play_spring_force(DEVICE_INDEX, 60, 50, 50)
	return

func _exit_tree():
	if logi_wheel:
		logi_wheel.shutdown()
