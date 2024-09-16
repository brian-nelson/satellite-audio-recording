import React from "react";
import { Route, Switch } from "react-router-dom";
import AppliedRoute from "./Components/AppliedRoute";
import AuthenticatedRoute from "./Components/AuthenticatedRoute";
import UnauthenticatedRoute from "./Components/UnauthenticatedRoute";
import Home from "./Containers/Home";
import Login from "./Containers/Login";
import Setup from "./Containers/Setup";
import Character from "./Containers/Character";
import CharacterLines from "./Containers/CharacterLines";
import Characters from "./Containers/Characters";
import Participant from "./Containers/Participant";
import Participants from "./Containers/Participants";
import Profile from "./Containers/Profile";
import Projects from "./Containers/Projects";
import Project from "./Containers/Project";
import RecordDialog from "./Containers/RecordDialog";
import SetPassword from "./Containers/SetPassword";
import Script from "./Containers/Script";
import User from "./Containers/User"
import Users from "./Containers/Users";
import NotFound from "./Containers/NotFound";

export default ({ childProps }) =>
  <Switch>
    <AppliedRoute path="/" exact component={Home} props={childProps} />
    <UnauthenticatedRoute path="/login" exact component={Login} props={childProps} />
    <UnauthenticatedRoute path="/setup" exact component={Setup} props={childProps} />
    <AuthenticatedRoute path="/projects" exact component={Projects} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId" exact component={Project} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/characters/:characterId" exact component={Character} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/characters/:characterId/lines" exact component={CharacterLines} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/script" exact component={Script} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/characters" exact component={Characters} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/participants" exact component={Participants} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/participants/:participantId" exact component={Participant} props={childProps} />
    <AuthenticatedRoute path="/projects/:projectId/dialog/:dialogId/record" exact component={RecordDialog} props={childProps} />
    <AuthenticatedRoute path="/profile" exact component={Profile} props={childProps} />
    <AuthenticatedRoute path="/users/:userId" exact component={User} props={childProps} />
    <AuthenticatedRoute path="/users/setpassword/:userId" exact component={SetPassword} props={childProps} />
    <AuthenticatedRoute path="/users" exact component={Users} props={childProps} />
    { /* Finally, catch all unmatched routes */ }
    <Route component={NotFound} />
  </Switch>;