import "./User.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Form, Button} from "react-bootstrap";
import { Link, Redirect } from 'react-router-dom'
import {v4 as uuid} from 'uuid';
import {toast} from "react-toastify";

export default class User extends Component {
  constructor(props) {
    super(props);

    let userId = this.props.match.params.userId;

    this.state = {
      redirect: false,
      userId: userId,
      personId:"",
      email: "",
      userType: "Contributor",
      givenName: "",
      familyName: "",
      phoneNumber: "",
      password: "",
      userTypes: [ "Owner", "Admin", "Contributor" ]
    };

    this.loadUser = this.loadUser.bind(this);
  }

  componentDidMount() {
    if (this.state.userId !== 'new') {
      this.loadUser(this.state.userId);
    }
  }

  loadUser(userId) {
    SarService.getUser(userId)
      .then(r => {
        this.setState({
          personId: r.PersonId,
          givenName: r.GivenName ?? '',
          familyName: r.FamilyName ?? '',
          email: r.Email ?? '',
          phoneNumber: r.PhoneNumber ?? '',
          userType: r.UserType ?? "Contributor"
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
      return <Redirect to={`/users`} />
    }
  };

  handleSubmit = event => {
    event.preventDefault();

    try {
      let userId = this.state.userId;
      if (userId==='new'){
        userId = uuid();
      }

      let personId = this.state.personId;
      if (personId === '') {
        personId = uuid();
      }

      let givenName = this.state.givenName;

      let familyName = this.state.familyName;
      if (familyName === "") {
        familyName = null;
      }

      let phoneNumber = this.state.phoneNumber;
      if (phoneNumber === "") {
        phoneNumber = null;
      }

      let email = this.state.email;
      if (email === "") {
        email = null;
      }

      let userType = this.state.userType;

      let user = {
        UserId: userId,
        PersonId: personId,
        GivenName: givenName,
        FamilyName: familyName,
        PhoneNumber: phoneNumber,
        Email: email,
        UserType: userType
      };

      SarService.saveUser(user)
        .then(r => {
          if (this.state.password !== '') {
            let spr = {
              NewPassword: this.state.password
            };

            SarService.setPassword(userId, spr)
              .then(r2 => {
                this.setState({
                  redirect: true
                });
              })
              .catch(e => {
                toast.error(e.message);
              })
          } else {
            this.setState({
              redirect: true
            });
          }
        })
        .catch(e => {
          toast.error(e.message);
        })
    } catch (e) {
      toast.error(e.message);
    }
  };

  renderSelect() {
    return(
      <Form.Group controlId="userType">
        <Form.Control as="select"
                      value={this.state.userType}
                      onChange={this.handleChange} >
          <option value="Contributor">Contributor</option>
          <option value="Admin">Admin</option>
          <option value="Owner">Owner</option>
        </Form.Control>
      </Form.Group>);
  }

  renderPassword() {
    if (this.state.userId === 'new'){
      return (
        <Form.Group controlId="password">
          <Form.Label>User Password</Form.Label>
          <Form.Control
            type="text"
            placeholder="Enter users's initial password"
            value={this.state.password}
            onChange={this.handleChange}
          />
        </Form.Group>);
    }
  }

  render() {
    return (
      <div className="User">
        {this.renderRedirect()}
        <Row>
          <Col>
            <h3>User: {`${this.state.givenName} ${this.state.familyName}`.trim()}</h3>
          </Col>
          <Col>
            <div className="float-md-right">
              <Link to={`/users`}>Return to Users</Link>
            </div>
          </Col>
        </Row>
        <Row>
          <Col>
            <Form onSubmit={this.handleSubmit}>
              <Form.Group controlId="givenName">
                <Form.Label>First Name</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter user given name"
                  value={this.state.givenName}
                  onChange={this.handleChange}
                />
              </Form.Group>
              <Form.Group controlId="familyName">
                <Form.Label>Last Name</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter user family name"
                  value={this.state.familyName}
                  onChange={this.handleChange}
                />
              </Form.Group>
              <Form.Group controlId="email">
                <Form.Label>Email</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter user's email address"
                  value={this.state.email}
                  onChange={this.handleChange}
                />
              </Form.Group>
              <Form.Group controlId="phoneNumber">
                <Form.Label>Phone Number</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter users's phone number"
                  value={this.state.phoneNumber}
                  onChange={this.handleChange}
                />
              </Form.Group>
              { this.renderPassword() }
              <Form.Group controlId="userTypes">
                <Form.Label>User Type</Form.Label>
                {this.renderSelect(this.state.userTypes)}
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