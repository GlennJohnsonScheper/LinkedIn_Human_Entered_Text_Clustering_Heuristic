# LinkedIn_Human_Entered_Text_Clustering_Heuristic

You know how annoying it is to search your candidate database
when it contains very diverse human entered Job Title strings.

The main idea demonstrated here is to represent each Job Title
by the bag / unique set of all its lowercase alphabetic tokens.

Then, remove the least frequent or valuable words one by one,
and merge together such bags/sets as are no longer different.

See how it merges many topics into the one topic 'security',
but not 'information/security' which words have more worth:


    4498 Topic Key: /security/
    3804 Security
     322 Security+
     194 National Security
      46 Homeland Security
      23 RSA Security
       9 Email Security
       5 Perimeter Security
       5 Logical Security
       4 SCADA Security
       4 CCNP Security
       3 Security Plus
       3 Personal Security
       3 MCSA Security
       3 Maritime Security
       3 Content Security
       2 Security Tools
       2 Security Review
       2 Security Protocols
       2 Security Onion
       2 Security Authorization
       2 Security Assertion Markup Language (SAML)
       2 IS Security
       2 Industrial Security
       2 Host Security
       1 Usable Security
       1 Security+ Certification
       1 security+
       1 Security Strategist
       1 Security Reviews
       1 Security Projects
       1 security plus
       1 Security Evaluations
       1 Security Countermeasures
       1 Security Coding
       1 Security Certification
       1 Security Architect
       1 SECURITY +
       1 Security +
       1 security +
       1 security
       1 SC Security Cleared
       1 Residential Security Course
       1 Port Security
       1 Pipeline Security
       1 PIN Security
       1 Panda Security
       1 Operation Security
       1 Omnicell User Security
       1 Offensive Security
       1 model-driven security
       1 Messaging Security
       1 Mesos Security
       1 MCSE Security 2003
       1 MCSE Security
       1 Mail Security
       1 Layer 7 Security
       1 Lawson User Security
       1 kernel security
       1 ICS Security
       1 GIAC (T1) (GSEC) SANS Security Essentials
       1 FireEYE NX security
       1 Final Security Review
       1 Facility Security Officer
       1 Energy Security
       1 End point security
       1 emissions security
       1 EHR Security
       1 Docker Security
       1 CVE, STIG, DISA guidelines and other security foci
       1 chief security officer
       1 CCNP-Security
       1 Barracuda Email Security
       1 Akamai Security

    3296 Topic Key: /information/security/
    3254 Information Security
      31 Information Security Consultancy
       2 Chief Information Security Officer
       1 Security Information
       1 Information Security Subject Matter Expert (SME)
       1 Information Security Practices and Procedures
       1 Information Security Instruction
       1 Information Security Architect
       1 INFORMATION SECURITY
       1 information security
       1 Develop New Information Security Programs
       1 Classified Information and Materials Security

This and other cleaning heuristics are in ClusterVocabulary.cs.
The rest of this project is just accessing a personal database.
