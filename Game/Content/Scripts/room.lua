-- local player = findEntity("hero")
-- local lamp = findEntity("Special Lamp")
-- local area = findEntity("Some Area")
-- local practiceArea = findEntity("practiceArea")

-- local testSwitch = vars["testSwitch"]

-- local test_dialog = cutscene(player, function()
--     dialog("This is long text to make sure text wrap algorithm doesn't completely screw up the dialog box output.")
--     dialog("The gate here will shut!")
--     testSwitch.Value = false
--     instantiate("Goblin", 170, 116)
-- end)

-- -- print("test debug log")

-- -- print(roomVars["str"])
-- -- print(roomVars["int"])
-- -- roomVars["int"] = "abc"
-- -- print(roomVars["int"])
-- -- roomVars["new"] = 4.0
-- -- print(roomVars["new"])

-- -- print(roomVars["strVal"])
-- -- print(roomVars["someBool"])

-- trigger(
--     onEnter(player, area),
--     test_dialog)

-- -- trigger(
-- --     onEnter(player, practiceArea),
-- --     cutscene(player, function()
-- --         dialog("This text is now edited.")
-- --     end)
-- -- )
