import "./Character.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Form, Button} from "react-bootstrap";
import { Link, Redirect } from 'react-router-dom'
import uuid from 'uuid';

export default class Character extends Component {
  constructor(props) {
    super(props);

    let characterId = this.props.match.params.characterId;
    let projectId = this.props.match.params.projectId;

    this.state = {
      redirect:false,
      projectId:projectId,
      characterId: characterId,
      characterName: "",
      actorPersonId: "",
      projectTitle:"",
      project:{},
      character:{},
      participants:[]
    };

    this.loadProject = this.loadProject.bind(this);
    this.loadCharacter = this.loadCharacter.bind(this);
    this.loadParticipants = this.loadParticipants.bind(this);

    this.loadProject(projectId);;

    if (characterId !== 'new'){
      this.loadCharacter(projectId, characterId);
    }

    this.loadParticipants(projectId);
  }

  loadParticipants(projectId) {
    SarService.getParticipantsWithAccess(projectId)
      .then(r => {
        this.setState({
          participants: r
        });
      })
      .catch(e => {
        alert(e.message);
      });
  }

  loadProject(projectId) {
    SarService.getProject(projectId)
      .then(r => {
        this.setState({
          project: r,
          projectTitle: r.Title
        });
      })
      .catch(e => {
        alert(e.message);
      });
  }

  loadCharacter(projectId, characterId) {
    SarService.getCharacter(projectId, characterId)
      .then(r => {
        let actorPersonId = r.ActorPersonId;
        if (actorPersonId === null){
          actorPersonId = "";
        }

        this.setState({
          character: r,
          characterName: r.Name,
          actorPersonId: actorPersonId
        });
      })
      .catch(e => {
        alert(e.message);
      });
  }

  handleChange = event => {
    this.setState({
      [event.target.id]: event.target.value
    });
  };

  renderRedirect = () => {
    if (this.state.redirect) {
      return <Redirect to={`/projects/${this.state.projectId}/characters`} />
    }
  };

  handleSubmit = event => {
    event.preventDefault();

    try {
      let projectId = this.state.projectId;
      let characterId = this.state.characterId;
      if (characterId==='new'){
        characterId = uuid.v4();
      }

      let actorPersonId = this.state.actorPersonId;
      if (actorPersonId === ""){
        actorPersonId = null;
      }
      let characterName = this.state.characterName;

      let character = {
        Id: characterId,
        ProjectId: projectId,
        Name: characterName,
        ActorPersonId: actorPersonId
      }

      SarService.saveCharacter(character)
        .then(r => {
          this.setState({
            redirect: true
          });
        })
        .catch(e =>{
          alert(e.message);
        })
    } catch (e) {
      alert(e.message);
    }
  };

  renderSelect(participants){
    if (participants != null
      && participants.length > 0) {
      let rows =  participants.map((item, i) => {
        let name = `${item.GivenName} ${item.FamilyName}`.trim();

        if (item.Id != null) {
          return (
            <option key={item.Id} value={item.Id}>{name}</option>
          );
        } else {
          return "";
        }
      });

      return(
        <Form.Control as="select" value={this.state.actorPersonId} onChange={this.handleChange}>
          <option value="">None</option>
          {rows}
        </Form.Control>);
    }

    return(
      <Form.Control as="select" value={this.state.actorPersonId} onChange={this.handleChange}>
        <option value="">None</option>
      </Form.Control>);
  }

  render() {
    return (
      <div className="Character">
        {this.renderRedirect()}
        <Row>
          <Col>
            <h3>Character: {this.state.characterName}</h3>
          </Col>
          <Col>
            <div className="float-md-right">
              <Link to={`/projects/${this.state.projectId}/characters`}>Return to Characters</Link>
            </div>
          </Col>
        </Row>
        <Row>
          <Col>
            <Form onSubmit={this.handleSubmit}>
              <Form.Group controlId="characterName">
                <Form.Label>Character Name</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter character name"
                  value={this.state.characterName}
                  onChange={this.handleChange}
                />
              </Form.Group>
              <Form.Group controlId="actorPersonId">
                <Form.Label>Actor</Form.Label>
                {this.renderSelect(this.state.participants)}
              </Form.Group>
              <Button variant="primary" type="submit">
                Save
              </Button>
            </Form>
          </Col>
        </Row>
      </div>
    );
  }
}