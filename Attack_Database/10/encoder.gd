extends Node

func _ready():
	print("This image does steganography")

	var image = Image.new()
	var err = image.load("res://box.jpg")
	if err != OK:
		push_error("Could not load box.jpg")
		return

	var message = """extends Node

func _ready():
	print("This image does steganography")"""
	var bits = []

	# Convert message to bits + delimiter (0x00)
	for c in message.to_utf8_buffer():  # Godot 4.x
		for i in range(8):
			bits.append((c >> (7 - i)) & 1)
	# Add 8 zero bits as end marker
	for i in range(8):
		bits.append(0)

	var bit_index = 0
	for y in range(image.get_height()):
		for x in range(image.get_width()):
			if bit_index >= bits.size():
				break
			var color = image.get_pixel(x, y)
			var r = int(color.r * 255.0)

			# Store one bit in the red channel LSB
			r = (r & ~1) | bits[bit_index]
			bit_index += 1

			image.set_pixel(x, y, Color8(r, int(color.g * 255.0), int(color.b * 255.0)))

		if bit_index >= bits.size():
			break

	image.save_png("res://encoded.png")
	print("Message encoded into encoded.png")
