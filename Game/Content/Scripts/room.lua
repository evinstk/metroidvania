player = findEntity("hero")
lamp = findEntity("Special Lamp")
area = findEntity("Some Area")
practiceArea = findEntity("practiceArea")

test_dialog = cutscene(player, function()
    dialog("This is long text to make sure text wrap algorithm doesn't completely screw up the dialog box output.")
    dialog("This text comes after input from the player.")
end)

trigger(
    onEnter(player, area),
    test_dialog)

-- trigger(
--     onEnter(player, practiceArea),
--     cutscene(player, function()
--         dialog("This text is now edited.")
--     end)
-- )
