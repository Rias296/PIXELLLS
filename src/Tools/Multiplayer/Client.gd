extends Node

var peer = ENetMultiplayerPeer.new()


func _ready():
	peer.create_client(, PORT)
	multiplayer.multiplayer_peer = peer

	multiplayer.connected_to_server.connect(_on_connected_to_server)


func _on_connected_to_server():
	print("Connection stablished!")
