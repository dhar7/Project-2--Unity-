@tool
extends EditorImportPlugin

func _get_importer_name():
	return "alg1.extractor"

func _get_visible_name():
	return "Algorithm 1 Extractor"

func _get_recognized_extensions():
	return ["txt", "png"]

func _get_save_extension():
	return "res"

func _get_resource_type():
	return "Resource"

func _get_priority():
	return 1.0

func _get_import_order():
	return 0

func _get_preset_count():
	return 0

# --- FIX: Added Missing Required Methods ---
func _get_import_options(path, preset_index):
	# We must return an array, even if empty
	return []

func _get_option_visibility(path, option_name, options):
	return true
# -------------------------------------------

func _import(source_file, save_path, options, platform_variants, gen_files):
	# 1. Notify that the Import Hook was triggered (The IMP)
	print("\n[Alg1] --- IMPORT INTERCEPTED ---")
	print("[Alg1] Target Asset: " + source_file)
	
	# 2. Attempt to get the stack
	var trace = get_stack()
	
	if trace.size() > 0:
		# If called by another script
		print("[Alg1] Execution Trace (Script):")
		for frame in trace:
			var m = frame["function"]
			var source = frame.get("source", "unknown")
			print("    > " + str(source) + " :: " + str(m))

		
	print("[Alg1] --- END ANALYSIS ---\n")

	# 4. Save a dummy resource to satisfy the pipeline
	return ResourceSaver.save(Resource.new(), save_path + "." + _get_save_extension())
