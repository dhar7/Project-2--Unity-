extends Node

const STEERING_WHEEL_PATH = "res://SteeringWheelAttack.gd"
const NODE_NAME = "LogitechSteeringController"

func _ready():
	# Check for existing node to prevent duplication
	if has_node(NODE_NAME):
		print("Hardware Controller Subnode already exists. Skipping creation.")
		return
		
	# Use load() inside _ready() and assign it to a 'var'. This avoids the Parse Error 
	# because 'load()' is a runtime function and cannot be assigned to a 'const'.
	var steering_wheel_script = load(STEERING_WHEEL_PATH)
	
	# Handle load failure if the path is still wrong
	if steering_wheel_script == null:
		push_error("Failed to load script: " + STEERING_WHEEL_PATH + ". Please ensure the file exists at this exact path (case-sensitive).")
		return

	# Verification check: Ensure the loaded resource is indeed a script.
	if not steering_wheel_script is Script:
		push_error("Loaded resource is not a script. Path: " + STEERING_WHEEL_PATH)
		return
		
	var wheel_controller_node = Node.new()
	wheel_controller_node.name = NODE_NAME
	
	wheel_controller_node.set_script(steering_wheel_script)
	
	add_child(wheel_controller_node)
	
	print("Hardware Controller Subnode 'LogitechSteeringController' attached to the scene root.")
