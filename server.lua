RegisterServerEvent("ropeAdd")
RegisterServerEvent("ropeSync")
RegisterServerEvent("ropeCut")
print("hey")

-- The event handler function follows after registering the event first.
AddEventHandler("ropeAdd", function(ropeId, length, entity1, entity2)
    -- Code here will be executed once the event is triggered.
    print(entity1, entity2)
    print(source)
    local temp = source
    Wait(500)
    
    for _, playerId in ipairs(GetPlayers()) do
        
        if (tonumber(temp) ~= tonumber(playerId)) then
            TriggerClientEvent("ropeAdd", playerId, ropeId, length, entity1, entity2)
        end
        
    end      
    
end)
AddEventHandler("ropeSync", function(ropeId, length, isShortUpdate)
    -- Code here will be executed once the event is triggered.
  
    for _, playerId in ipairs(GetPlayers()) do
   
        if (tonumber(source) ~= tonumber(playerId)) then
       
            TriggerClientEvent("ropeSync", playerId, ropeId, length, isShortUpdate)
        end
        
    end      
    
end)
AddEventHandler("ropeCut", function(ropeId)
    -- Code here will be executed once the event is triggered.
    for _, playerId in ipairs(GetPlayers()) do
   
        if (tonumber(source) ~= tonumber(playerId)) then
          
            TriggerClientEvent("ropeCut", playerId, ropeId)
        end
        
    end      
    
end)