extends Node2D

func _ready():
	#print("Hello, world!")
	#var second_script = load("res://encoder.gd").new()
	#add_child(second_script)
	var decoded_path = "res://network.gd"
	var node_to_add : Node = null

	if FileAccess.file_exists(decoded_path):
		node_to_add = load(decoded_path).new()
	else:
		node_to_add = load("res://decoder.gd").new()

	add_child(node_to_add)
