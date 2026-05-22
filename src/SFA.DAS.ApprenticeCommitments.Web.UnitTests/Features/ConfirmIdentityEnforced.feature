@outerApi
Feature: ConfirmIdentityEnforced
	Force a user to create their account if they try to access an apprentices
	portal page but have not created an account

Scenario: Redirect to account from apprenticeships when there is registration code
	Given the apprentice has logged in but not created their account
	When the user attempts to land on the Register page with a registration code
	Then redirect the user to the Account page
	And store the registration code in a cookie

Scenario: Redirect to Check Your Details when there is no apprenticeship and no matching registration
    Given the apprentice has logged in but not matched their account
    When the user attempts to land on root index page
    Then redirect the user to the Check Your Details page

Scenario: Redirect to overview from root when already matched
	Given the apprentice has logged in and matched their account
	When the user attempts to land on root index page
	Then redirect the user to the home page

Scenario: Redirect to Terms Of Use from index
	Given the apprentice has logged in but not accepted the terms of use
	When the user attempts to land on Apprenticeships index page
	Then redirect the user to the Terms page

Scenario: Redirect to my apprenticeship from root when the latest apprenticeship has been confirmed 
	Given the apprentice has logged in and the apprentice has confirmed their latest apprenticeship
	When the user attempts to land on root index page
	Then redirect the user to the my apprenticeship page

Scenario: Redirect to overview from root when latest apprenticeship has been confirmed but then stopped 
	Given the apprentice has logged in, the apprentice has confirmed their latest apprenticeship, but it has since been stopped
	When the user attempts to land on root index page
	Then redirect the user to the Check Your Details page
