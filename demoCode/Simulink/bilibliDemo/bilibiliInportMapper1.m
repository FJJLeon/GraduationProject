% -------------------------------------------------------------------
% Generated by MATLAB on 20-Apr-2020 00:04:16
% MATLAB version: 9.6.0.1335978 (R2019a) Update 8
% -------------------------------------------------------------------
% While generating this script, Root Inport Mapper ignored inputs without connections.
simOut = runSimulation(); 

function simOut = runSimulation() 
% Create a cell array storing the file names of the input data 
cellOfFiles = { ...
'bilibiliExData1.mat' , ...
}; 

% Create a cell array storing the variables names of the scenario data 
cellOfVarNames = { ...
'bilibiliExData1' , ...
}; 

% Create a cell array storing the input strings for each scenario 
cellOfInputStrings = { ...
'bilibiliExData1.getElement(1)' , ...
}; 

cellOfErrors = cell(1,length( cellOfInputStrings ));
simOut = Simulink.SimulationOutput.empty(0,length( cellOfInputStrings ));
simIn  = Simulink.SimulationInput.empty(0,length( cellOfInputStrings ));

% For each scenario 
for kScenario = 1: length( cellOfInputStrings ) 
	 try 
	 	% Load Variable from file 
	 	loadScenarioToWorkspace( cellOfFiles{kScenario},cellOfVarNames{kScenario}); 
	 	% set up SimulationInput object 
	 	simIn(kScenario) = Simulink.SimulationInput('bilibiliExample');
	 	simIn(kScenario).ExternalInput = cellOfInputStrings{kScenario};
	 	simIn(kScenario) = simIn(kScenario).setVariable( cellOfVarNames{kScenario}, evalin('base',(cellOfVarNames{kScenario})) );
	 	evalin('base',sprintf('clear %s',cellOfVarNames{kScenario}));
	 catch ME 
	 	cellOfErrors{ length(cellOfErrors) + 1 } = ME.message; 
	 end 
end 
% End for each scenario 

if ~isempty( simIn )
	simOut = parsim(simIn);
end 
% Report Errors 
idxEmpty = cellfun(@isempty, cellOfErrors); 
cellOfErrors(idxEmpty)= []; 
if ~isempty(cellOfErrors) 

	disp('These errors occurred while running this script.');
	% For each error 
	for kErr = 1: length( cellOfErrors ) 
		disp(cellOfErrors{kErr}); 
	end 
	% End for each error 

end 
end 
