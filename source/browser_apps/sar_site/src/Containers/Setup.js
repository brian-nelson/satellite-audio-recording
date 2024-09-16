import "./Setup.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Form, Button} from "react-bootstrap";
import { Redirect } from 'react-router-dom'
import {toast} from "react-toastify";

export default class Setup extends Component {
  constructor(props) {
    super(props);

    this.state = {
      isSetup: true,
      redirectToHome: false,
      ownerEmail:"",
      ownerPassword:"",
      ownerGivenName:"",
      ownerFamilyName:"",
      initialProjectName:""
    };
  }

  componentDidMount() {
    SarService.isSetup()
      .then(r => {
        if (r.IsSetup) {
          this.setState({
            redirectToHome: true
          });
        } else {
          this.setState({
            isSetup:false
          });
        }
      }).catch(e => {
      toast.error(e.message);
    });
  }

  handleChange = event => {
    this.setState({
      [event.target.id]: event.target.value
    });
  };

  handleSubmit = event => {
    event.preventDefault();

    try {
      SarService.setupService(
        this.state.ownerEmail,
        this.state.ownerPassword,
        this.state.ownerGivenName,
        this.state.ownerFamilyName,
        this.state.initialProjectName)
        .then(r => {
          if (r.IsSetup) {
            this.setState({
              isSetup:true,
              redirectToHome:true
            });
          }
        })
        .catch(e =>{
          toast.error(e.message);
        })
    } catch (e) {
      toast.error(e.message);
    }
  };

  renderSetup() {
    return (
      <Row>
        <Col>
          <Row>
            <Col>
              <h2>Setup</h2>
            </Col>
          </Row>
          <Row>
            <Col>
              <Form onSubmit={this.handleSubmit}>
                <Form.Group controlId="ownerEmail">
                  <Form.Label>Owner Email</Form.Label>
                  <Form.Control
                    type="email"
                    value={this.state.ownerEmail}
                    onChange={this.handleChange}
                    placeholder="Enter email"/>
                </Form.Group>
                <Form.Group controlId="ownerPassword">
                  <Form.Label>Owner Password</Form.Label>
                  <Form.Control
                    type="password"
                    value={this.state.ownerPassword}
                    onChange={this.handleChange}
                    placeholder="Enter password"/>
                </Form.Group>
                <Form.Group controlId="ownerGivenName">
                  <Form.Label>Owner First Name</Form.Label>
                  <Form.Control
                    type="text"
                    value={this.state.ownerGivenName}
                    onChange={this.handleChange}
                    placeholder="Enter first name"/>
                </Form.Group>
                <Form.Group controlId="ownerFamilyName">
                  <Form.Label>Owner Last Name</Form.Label>
                  <Form.Control
                    type="text"
                    value={this.state.ownerFamilyName}
                    onChange={this.handleChange}
                    placeholder="Enter last name"/>
                </Form.Group>
                <Form.Group controlId="initialProjectName">
                  <Form.Label>Project Title</Form.Label>
                  <Form.Control
                    type="text"
                    value={this.state.initialProjectName}
                    onChange={this.handleChange}
                    placeholder="Enter initial project title"/>
                </Form.Group>
                <Button variant="primary" type="submit">
                  Setup
                </Button>
              </Form>
            </Col>
          </Row>
        </Col>
      </Row>
    );
  }

  renderEmpty() {
    return(<Row></Row>);
  }

  renderRedirect = () => {
    if (this.state.redirectToHome) {
      return <Redirect to={'/'} />
    }
  };

  render() {
    return (
      <div className="Setup">
        {this.renderRedirect()}
        {this.state.isSetup ? this.renderEmpty() : this.renderSetup()}
      </div>
    );
  }
}