--- chest contents
vars['default_chest_contents'] = data.contents {
    { 'Guard Baton', 1 },
    { 'Blaster', 1 },
}

vars['baton_chest_contents'] = data.contents {
    { 'Guard Baton', 1 },
}

vars['blaster_chest_contents'] = data.contents {
    { 'Blaster', 1 },
}

--- switches
vars['default_switch_can_use'] = function()
    return true
end

vars['blaster_privileges'] = function()
    -- TODO: flip this with key card or something
    return false
end

vars['blaster_gate_condition'] = function()
    return vars['blaster_gate'] or vars['blaster_privileges']()
end

--- common
vars['always_true'] = function()
    return true
end

vars['always_false'] = function()
    return false
end

vars['t'] = true
vars['f'] = false
