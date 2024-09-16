import "./Character.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Form, Button} from "react-bootstrap";
import { Link, Redirect } from 'react-router-dom'
import {v4 as uuid} from 'uuid';
import {toast} from "react-toastify";

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
      performerPersonId: "",
      projectTitle:"",
      project:{},
      character:{},
      participants:[]
    };

    this.loadProject = this.loadProject.bind(this);
    this.loadCharacter = this.loadCharacter.bind(this);
    this.loadParticipants = this.loadParticipants.bind(this);
  }

  componentDidMount() {
    this.loadProject(this.state.projectId);

    if (this.state.characterId !== 'new'){
      this.loadCharacter(this.state.projectId, this.state.characterId);
    }

    this.loadParticipants(this.state.projectId);
  }

  loadParticipants(projectId) {
    SarService.getParticipantsWithAccess(projectId)
      .then(r => {
        this.setState({
          participants: r
        });
      })
      .catch(e => {
        toast.error(e.message);
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
        toast.error(e.message);
      });
  }

  loadCharacter(projectId, characterId) {
    SarService.getCharacter(projectId, characterId)
      .then(r => {
        let performerPersonId = r.PerformerPersonId;
        if (performerPersonId === null){
          performerPersonId = "";
        }

        this.setState({
          character: r,
          characterName: r.Name,
          performerPersonId: performerPersonId
        });
      })
      .catch(e => {
        toast.error(e.message);
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
        characterId = uuid();
      }

      let performerPersonId = this.state.performerPersonId;
      if (performerPersonId === ""){
        performerPersonId = null;
      }
      let characterName = this.state.characterName;

      let character = {
        Id: characterId,
        ProjectId: projectId,
        Name: characterName,
        performerPersonId: performerPersonId
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
        <Form.Control as="select" value={this.state.performerPersonId} onChange={this.handleChange}>
          <option value="">None</option>
          {rows}
        </Form.Control>);
    }

    return(
      <Form.Control as="select" value={this.state.performerPersonId} onChange={this.handleChange}>
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
              <Form.Group controlId="performerPersonId">
                <Form.Label>Performer</Form.Label>
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