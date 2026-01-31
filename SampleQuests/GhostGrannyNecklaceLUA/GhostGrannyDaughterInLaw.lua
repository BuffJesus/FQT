-- GhostGrannyDaughterInLaw.lua
-- This script will control the behavior of the son's wife.

Quest = nil
Me = nil

---------------------------------------------------------------------
-- INIT: Called once when the entity is loaded.
---------------------------------------------------------------------
function Init(questObject, meObject)
    Quest = questObject
    Me = meObject
    Quest:Log("GhostGrannyDaughterInLaw '" .. Me:GetDataString() .. "' initialized.")
end

---------------------------------------------------------------------
-- MAIN: The main lifecycle loop for this entity.
---------------------------------------------------------------------
function Main(questObject, meObject)
    Quest = questObject
    Me = meObject

    -- This infinite loop keeps the script alive.
    -- Logic will be added here in a later step.
    while true do
        -- Wait for one frame before running again.
        if not Quest:Wait(0) then
            break
        end
    end
end