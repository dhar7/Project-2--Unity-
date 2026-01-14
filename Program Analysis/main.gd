@tool
extends EditorPlugin

var import_plugin

func _enter_tree():
	# DEBUG: Print in RED to confirm the plugin loaded
	printerr("[Alg1-Main] STATUS: Plugin Enabled/Loaded.")
	
	import_plugin = preload("res://addons/alg1_extractor/alg1_import_plugin.gd").new()
	add_import_plugin(import_plugin)

func _exit_tree():
	if import_plugin:
		remove_import_plugin(import_plugin)
		import_plugin = null
	printerr("[Alg1-Main] STATUS: Plugin Disabled.")
