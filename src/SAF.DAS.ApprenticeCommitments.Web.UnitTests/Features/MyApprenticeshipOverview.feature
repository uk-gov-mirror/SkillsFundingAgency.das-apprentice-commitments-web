﻿@outerApi
Feature: MyApprenticeshipOverview

Feature: MyApprenticeships
	As an apprentice
	I need to have a start screen showing all the steps of my commitment statement
	So that I know what steps are involved in signing my commitment statement
	And I can be directed to sign every aspect of my statement

Scenario: The apprentice is authenticated and should see the overview page
	Given the apprentice has logged in
	And the apprentice will navigate to the overview page
	When accessing the overview page
	Then the response status code should be Ok
	Then the apprentice should see the apprenticeship overview page for the apprenticeship

Scenario: The apprentice has not confirmed every aspect of the apprenciceship
	Given the apprentice has logged in
	And the apprentice has not confirmed every aspect of the apprenciceship	
	And the apprentice will navigate to the overview page
	When accessing the overview page
	Then the response status code should be Ok
	Then the apprentice should not see the ready to confirm banner

Scenario: The apprentice has confirmed every aspect of the apprenciceship
	Given the apprentice has logged in
	And the apprentice has confirmed every aspect of the apprenciceship	
	And the apprentice will navigate to the overview page
	When accessing the overview page
	Then the response status code should be Ok
	Then the apprentice should see the ready to confirm banner
