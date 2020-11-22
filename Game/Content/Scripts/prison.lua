player = findEntity("player")
leaveTrigger = findEntity("leaveTrigger")
npc = findEntity("NPC")
leaveZone = findEntity("leaveZone")

trigger(
	onEnter(player, leaveTrigger),
	moveCommand(npc, leaveZone))

trigger(
	onEnter(npc, leaveZone),
	destroyCommand(npc))
